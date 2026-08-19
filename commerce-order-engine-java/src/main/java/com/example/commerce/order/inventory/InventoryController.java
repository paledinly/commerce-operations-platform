package com.example.commerce.order.inventory;

import static com.example.commerce.order.inventory.InventoryModels.*;
import jakarta.validation.Valid;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import java.util.Map;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/internal/inventories")
public class InventoryController {
    private final InventoryService service;
    public InventoryController(InventoryService service) { this.service = service; }
    @GetMapping public InventoryPage list(@RequestParam(defaultValue="1") @Min(1) int page, @RequestParam(defaultValue="20") @Min(1) @Max(100) int pageSize) { return service.list(page, pageSize); }
    @GetMapping("/{productId}") public Inventory get(@PathVariable long productId) { return service.get(productId); }
    @GetMapping("/{productId}/movements") public java.util.List<Movement> movements(@PathVariable long productId) { return service.movements(productId); }
    @PostMapping @ResponseStatus(HttpStatus.CREATED) public Inventory create(@Valid @RequestBody CreateInventoryRequest request) { return service.create(request); }
    @PostMapping("/{productId}/adjustments") public Inventory adjust(@PathVariable long productId, @Valid @RequestBody AdjustInventoryRequest request) { return service.adjust(productId, request); }
}

@RestControllerAdvice
class InventoryExceptionHandler {
    @ExceptionHandler(InventoryNotFoundException.class) @ResponseStatus(HttpStatus.NOT_FOUND) Map<String,String> notFound(RuntimeException exception) { return Map.of("title", exception.getMessage()); }
    @ExceptionHandler(InventoryConflictException.class) @ResponseStatus(HttpStatus.CONFLICT) Map<String,String> conflict(RuntimeException exception) { return Map.of("title", exception.getMessage()); }
}

