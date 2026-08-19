package com.example.commerce.order.orders;

import static com.example.commerce.order.orders.OrderModels.*;
import java.util.*;
import java.util.concurrent.TimeUnit;
import org.redisson.api.*;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import com.fasterxml.jackson.databind.ObjectMapper;

@Service
public class OrderService {
    private final OrderRepository repository; private final RedissonClient redisson; private final ObjectMapper json;
    public OrderService(OrderRepository repository,RedissonClient redisson,ObjectMapper json){this.repository=repository;this.redisson=redisson;this.json=json;}
    public OrderPage list(int p,int s){return repository.findAll(p,s);} public Order get(long id){return repository.find(id).orElseThrow(()->new OrderNotFoundException());}
    @Transactional public Order create(CreateOrderRequest request){
        var merged=new LinkedHashMap<Long,CreateItem>();
        for(var i:request.items()){var old=merged.get(i.productId());merged.put(i.productId(),old==null?i:new CreateItem(i.productId(),i.sku(),i.productName(),i.unitPrice(),Math.addExact(old.quantity(),i.quantity())));}
        var normalized=new CreateOrderRequest(request.customerId(),request.customerEmail(),request.customerName(),new ArrayList<>(merged.values()));
        return locked(merged.keySet(),()->{long id=repository.create(normalized);for(var i:normalized.items()){if(repository.reserve(i.productId(),i.quantity())==0)throw new OrderConflictException("Insufficient inventory for product "+i.productId());repository.movement(i.productId(),"RESERVATION",-i.quantity(),repository.available(i.productId()),"Order #"+id+" reservation");}event(id,"ORDER_CREATED");return get(id);});
    }
    @Transactional public Order cancel(long id){var order=get(id);if(!order.status().equals("CREATED"))throw new OrderConflictException("Order cannot be cancelled");return locked(order.items().stream().map(OrderItem::productId).toList(),()->{if(repository.cancel(id)==0)throw new OrderConflictException("Order cannot be cancelled");for(var i:order.items()){if(repository.release(i.productId(),i.quantity())==0)throw new OrderConflictException("Reserved inventory is inconsistent");repository.movement(i.productId(),"RELEASE",i.quantity(),repository.available(i.productId()),"Order #"+id+" cancellation");}return get(id);});}
    @Transactional public Payment pay(long id){var order=get(id);if(repository.changeStatus(id,"CREATED","PAID")==0)throw new OrderConflictException("Only created orders can be paid");var payment=repository.addPayment(id,"PAYMENT",order.totalAmount());event(id,"ORDER_PAID");return payment;}
    @Transactional public Payment refund(long id){var order=get(id);if(!order.status().equals("PAID"))throw new OrderConflictException("Only paid orders can be refunded");return locked(order.items().stream().map(OrderItem::productId).toList(),()->{if(repository.changeStatus(id,"PAID","REFUNDED")==0)throw new OrderConflictException("Order cannot be refunded");for(var i:order.items()){if(repository.release(i.productId(),i.quantity())==0)throw new OrderConflictException("Reserved inventory is inconsistent");repository.movement(i.productId(),"RELEASE",i.quantity(),repository.available(i.productId()),"Order #"+id+" refund");}var payment=repository.addPayment(id,"REFUND",order.totalAmount());event(id,"ORDER_REFUNDED");return payment;});}
    @Transactional public Shipment ship(long id,ShipOrderRequest request){var order=get(id);if(!order.status().equals("PAID"))throw new OrderConflictException("Only paid orders can be shipped");return locked(order.items().stream().map(OrderItem::productId).toList(),()->{if(repository.changeStatus(id,"PAID","SHIPPED")==0)throw new OrderConflictException("Order cannot be shipped");for(var i:order.items()){if(repository.fulfill(i.productId(),i.quantity())==0)throw new OrderConflictException("Reserved inventory is inconsistent");repository.movement(i.productId(),"FULFILLMENT",0,repository.available(i.productId()),"Order #"+id+" shipment");}var shipment=repository.addShipment(id,request);event(id,"ORDER_SHIPPED");return shipment;});}
    @Transactional public Shipment deliver(long id){if(repository.changeStatus(id,"SHIPPED","COMPLETED")==0)throw new OrderConflictException("Only shipped orders can be completed");if(repository.deliver(id)==0)throw new OrderConflictException("Shipment is inconsistent");event(id,"ORDER_COMPLETED");return repository.shipmentForOrder(id);}
    private void event(long id,String type){try{repository.outbox(id,type,json.writeValueAsString(Map.of("orderId",id,"eventType",type)));}catch(Exception e){throw new IllegalStateException(e);}}
    private <T>T locked(Collection<Long> ids,java.util.concurrent.Callable<T> action){var locks=ids.stream().distinct().sorted().map(id->redisson.getLock("inventory:"+id)).toList();var acquired=new ArrayList<RLock>();try{for(var lock:locks){if(!lock.tryLock(5,30,TimeUnit.SECONDS))throw new OrderConflictException("Inventory is busy; retry later");acquired.add(lock);}return action.call();}catch(InterruptedException e){Thread.currentThread().interrupt();throw new OrderConflictException("Inventory lock interrupted");}catch(OrderNotFoundException|OrderConflictException e){throw e;}catch(Exception e){throw new IllegalStateException(e);}finally{for(int i=acquired.size()-1;i>=0;i--){var l=acquired.get(i);if(l.isHeldByCurrentThread())l.unlock();}}}
}
class OrderNotFoundException extends RuntimeException {}
class OrderConflictException extends RuntimeException { OrderConflictException(String message){super(message);} }
