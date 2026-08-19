package com.example.commerce.order.orders;
import static com.example.commerce.order.orders.OrderModels.*;import static org.junit.jupiter.api.Assertions.assertThrows;import static org.mockito.Mockito.*;
import java.math.BigDecimal;import java.util.List;import java.util.concurrent.TimeUnit;import org.junit.jupiter.api.Test;import org.redisson.api.*;
class OrderServiceTests{
 @Test void insufficient_inventory_rejects_order()throws InterruptedException{var repo=mock(OrderRepository.class);var redisson=mock(RedissonClient.class);var lock=mock(RLock.class);when(redisson.getLock("inventory:10")).thenReturn(lock);when(lock.tryLock(5,30,TimeUnit.SECONDS)).thenReturn(true);when(lock.isHeldByCurrentThread()).thenReturn(true);when(repo.create(any())).thenReturn(7L);when(repo.reserve(10,3)).thenReturn(0);var service=new OrderService(repo,redisson,new com.fasterxml.jackson.databind.ObjectMapper());var request=new CreateOrderRequest(1,"customer@example.com","Customer",List.of(new CreateItem(10,"SKU","Product",BigDecimal.TEN,3)));assertThrows(OrderConflictException.class,()->service.create(request));}
}
