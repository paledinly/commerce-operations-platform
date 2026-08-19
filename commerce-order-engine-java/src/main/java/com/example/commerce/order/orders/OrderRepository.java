package com.example.commerce.order.orders;

import static com.example.commerce.order.orders.OrderModels.*;
import java.math.BigDecimal;
import java.sql.*;
import java.util.*;
import org.springframework.jdbc.core.namedparam.*;
import org.springframework.stereotype.Repository;

@Repository
public class OrderRepository {
    private final NamedParameterJdbcTemplate jdbc;
    public OrderRepository(NamedParameterJdbcTemplate jdbc) { this.jdbc = jdbc; }
    public OrderPage findAll(int page, int pageSize) {
        var p=Map.of("limit",pageSize,"offset",(page-1)*pageSize);
        var items=jdbc.query("SELECT * FROM orders ORDER BY created_at DESC,id DESC LIMIT :limit OFFSET :offset",p,this::summary);
        var total=jdbc.queryForObject("SELECT COUNT(*) FROM orders",Map.of(),Long.class);
        return new OrderPage(items,page,pageSize,total==null?0:total);
    }
    public Optional<Order> find(long id) {
        var rows=jdbc.query("SELECT * FROM orders WHERE id=:id",Map.of("id",id),(rs,n)->new Order(rs.getLong("id"),rs.getLong("customer_id"),rs.getString("customer_email"),rs.getString("customer_name"),rs.getString("status"),rs.getBigDecimal("total_amount"),rs.getTimestamp("created_at").toInstant(),rs.getTimestamp("updated_at").toInstant(),List.of()));
        if(rows.isEmpty()) return Optional.empty();
        var o=rows.getFirst();
        var items=jdbc.query("SELECT * FROM order_items WHERE order_id=:id ORDER BY id",Map.of("id",id),this::item);
        return Optional.of(new Order(o.id(),o.customerId(),o.customerEmail(),o.customerName(),o.status(),o.totalAmount(),o.createdAt(),o.updatedAt(),items));
    }
    public long create(CreateOrderRequest request) {
        BigDecimal total=request.items().stream().map(i->i.unitPrice().multiply(BigDecimal.valueOf(i.quantity()))).reduce(BigDecimal.ZERO,BigDecimal::add);
        var key=new org.springframework.jdbc.support.GeneratedKeyHolder();
        jdbc.update("INSERT INTO orders(customer_id,customer_email,customer_name,status,total_amount) VALUES(:customerId,:email,:name,'CREATED',:total)",new MapSqlParameterSource().addValue("customerId",request.customerId()).addValue("email",request.customerEmail()).addValue("name",request.customerName()).addValue("total",total),key);
        long id=Objects.requireNonNull(key.getKey()).longValue();
        for(var i:request.items()) jdbc.update("INSERT INTO order_items(order_id,product_id,sku,product_name,unit_price,quantity,line_amount) VALUES(:orderId,:productId,:sku,:name,:price,:quantity,:amount)",Map.of("orderId",id,"productId",i.productId(),"sku",i.sku(),"name",i.productName(),"price",i.unitPrice(),"quantity",i.quantity(),"amount",i.unitPrice().multiply(BigDecimal.valueOf(i.quantity()))));
        return id;
    }
    public int reserve(long productId,long quantity) { return jdbc.update("UPDATE inventories SET available_quantity=available_quantity-:quantity,reserved_quantity=reserved_quantity+:quantity,version=version+1 WHERE product_id=:productId AND available_quantity>=:quantity",Map.of("productId",productId,"quantity",quantity)); }
    public int release(long productId,long quantity) { return jdbc.update("UPDATE inventories SET available_quantity=available_quantity+:quantity,reserved_quantity=reserved_quantity-:quantity,version=version+1 WHERE product_id=:productId AND reserved_quantity>=:quantity",Map.of("productId",productId,"quantity",quantity)); }
    public long available(long productId) { var q=jdbc.queryForObject("SELECT available_quantity FROM inventories WHERE product_id=:id",Map.of("id",productId),Long.class); return q==null?0:q; }
    public void movement(long productId,String type,long delta,long after,String reason) { jdbc.update("INSERT INTO inventory_movements(product_id,movement_type,quantity_delta,available_after,reason) VALUES(:id,:type,:delta,:after,:reason)",Map.of("id",productId,"type",type,"delta",delta,"after",after,"reason",reason)); }
    public int cancel(long id) { return jdbc.update("UPDATE orders SET status='CANCELLED' WHERE id=:id AND status='CREATED'",Map.of("id",id)); }
    public int changeStatus(long id,String from,String to){return jdbc.update("UPDATE orders SET status=:to WHERE id=:id AND status=:from",Map.of("id",id,"from",from,"to",to));}
    public Payment addPayment(long orderId,String type,BigDecimal amount){var ref=type+"-"+orderId+"-"+UUID.randomUUID();var key=new org.springframework.jdbc.support.GeneratedKeyHolder();jdbc.update("INSERT INTO payment_transactions(order_id,transaction_type,amount,status,reference_no) VALUES(:orderId,:type,:amount,'APPROVED',:ref)",new MapSqlParameterSource().addValue("orderId",orderId).addValue("type",type).addValue("amount",amount).addValue("ref",ref),key);return payment(Objects.requireNonNull(key.getKey()).longValue());}
    public Payment payment(long id){return jdbc.queryForObject("SELECT * FROM payment_transactions WHERE id=:id",Map.of("id",id),(r,n)->new Payment(r.getLong("id"),r.getLong("order_id"),r.getString("transaction_type"),r.getBigDecimal("amount"),r.getString("status"),r.getString("reference_no"),r.getTimestamp("created_at").toInstant()));}
    public void outbox(long orderId,String eventType,String payload){jdbc.update("INSERT INTO order_event_outbox(order_id,event_type,payload) VALUES(:id,:type,CAST(:payload AS JSON))",Map.of("id",orderId,"type",eventType,"payload",payload));}
    public int fulfill(long productId,long quantity){return jdbc.update("UPDATE inventories SET reserved_quantity=reserved_quantity-:quantity,version=version+1 WHERE product_id=:productId AND reserved_quantity>=:quantity",Map.of("productId",productId,"quantity",quantity));}
    public Shipment addShipment(long orderId,ShipOrderRequest request){var key=new org.springframework.jdbc.support.GeneratedKeyHolder();jdbc.update("INSERT INTO shipments(order_id,carrier,tracking_number,status) VALUES(:orderId,:carrier,:tracking,'SHIPPED')",new MapSqlParameterSource().addValue("orderId",orderId).addValue("carrier",request.carrier().trim()).addValue("tracking",request.trackingNumber().trim()),key);return shipment(Objects.requireNonNull(key.getKey()).longValue());}
    public Shipment shipment(long id){return jdbc.queryForObject("SELECT * FROM shipments WHERE id=:id",Map.of("id",id),this::mapShipment);}
    public Shipment shipmentForOrder(long orderId){return jdbc.queryForObject("SELECT * FROM shipments WHERE order_id=:id",Map.of("id",orderId),this::mapShipment);}
    public int deliver(long orderId){return jdbc.update("UPDATE shipments SET status='DELIVERED',delivered_at=CURRENT_TIMESTAMP WHERE order_id=:id AND status='SHIPPED'",Map.of("id",orderId));}
    private OrderSummary summary(ResultSet r,int n)throws SQLException{return new OrderSummary(r.getLong("id"),r.getLong("customer_id"),r.getString("customer_email"),r.getString("customer_name"),r.getString("status"),r.getBigDecimal("total_amount"),r.getTimestamp("created_at").toInstant(),r.getTimestamp("updated_at").toInstant());}
    private OrderItem item(ResultSet r,int n)throws SQLException{return new OrderItem(r.getLong("id"),r.getLong("product_id"),r.getString("sku"),r.getString("product_name"),r.getBigDecimal("unit_price"),r.getLong("quantity"),r.getBigDecimal("line_amount"));}
    private Shipment mapShipment(ResultSet r,int n)throws SQLException{var delivered=r.getTimestamp("delivered_at");return new Shipment(r.getLong("id"),r.getLong("order_id"),r.getString("carrier"),r.getString("tracking_number"),r.getString("status"),r.getTimestamp("shipped_at").toInstant(),delivered==null?null:delivered.toInstant());}
}
