package com.example.commerce.order.orders;
import static com.example.commerce.order.orders.OrderModels.*;
import jakarta.validation.Valid;
import org.springframework.http.*;import org.springframework.web.bind.annotation.*;
@RestController @RequestMapping("/internal/orders")
public class OrderController { private final OrderService service; public OrderController(OrderService service){this.service=service;}
 @GetMapping public OrderPage list(@RequestParam(defaultValue="1") int page,@RequestParam(defaultValue="20") int pageSize){return service.list(page,pageSize);}
 @GetMapping("/{id}") public Order get(@PathVariable long id){return service.get(id);}
 @PostMapping public ResponseEntity<Order> create(@Valid @RequestBody CreateOrderRequest request){var o=service.create(request);return ResponseEntity.status(201).body(o);}
 @PostMapping("/{id}/cancel") public Order cancel(@PathVariable long id){return service.cancel(id);}
 @PostMapping("/{id}/pay") public Payment pay(@PathVariable long id){return service.pay(id);}
 @PostMapping("/{id}/refund") public Payment refund(@PathVariable long id){return service.refund(id);}
 @PostMapping("/{id}/ship") public Shipment ship(@PathVariable long id,@Valid @RequestBody ShipOrderRequest request){return service.ship(id,request);}
 @PostMapping("/{id}/deliver") public Shipment deliver(@PathVariable long id){return service.deliver(id);}
}
@RestControllerAdvice(assignableTypes=OrderController.class)
class OrderAdvice { @ExceptionHandler(OrderNotFoundException.class) ResponseEntity<?> missing(){return ResponseEntity.notFound().build();} @ExceptionHandler(OrderConflictException.class) ResponseEntity<?> conflict(OrderConflictException e){return ResponseEntity.status(409).body(java.util.Map.of("message",e.getMessage()));} }
