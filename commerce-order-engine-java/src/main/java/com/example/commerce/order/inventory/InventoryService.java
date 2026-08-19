package com.example.commerce.order.inventory;

import static com.example.commerce.order.inventory.InventoryModels.*;
import java.util.List;
import java.util.concurrent.TimeUnit;
import org.redisson.api.RedissonClient;
import org.springframework.dao.DuplicateKeyException;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
public class InventoryService {
    private final InventoryRepository repository;
    private final RedissonClient redisson;
    public InventoryService(InventoryRepository repository, RedissonClient redisson) { this.repository = repository; this.redisson = redisson; }
    public InventoryPage list(int page, int pageSize) { return repository.findAll(page, pageSize); }
    public Inventory get(long productId) { return repository.find(productId).orElseThrow(() -> new InventoryNotFoundException(productId)); }
    public List<Movement> movements(long productId) { get(productId); return repository.movements(productId); }

    @Transactional
    public Inventory create(CreateInventoryRequest request) {
        return withLock(request.productId(), () -> {
            try { repository.create(request.productId(), request.initialQuantity()); }
            catch (DuplicateKeyException exception) { throw new InventoryConflictException("Inventory already exists"); }
            return get(request.productId());
        });
    }

    @Transactional
    public Inventory adjust(long productId, AdjustInventoryRequest request) {
        return withLock(productId, () -> {
            get(productId);
            if (repository.adjust(productId, request.quantityDelta()) == 0) throw new InventoryConflictException("Available quantity cannot become negative");
            var updated = get(productId);
            repository.addMovement(productId, request.quantityDelta(), updated.availableQuantity(), request.reason());
            return updated;
        });
    }

    private <T> T withLock(long productId, java.util.concurrent.Callable<T> action) {
        var lock = redisson.getLock("inventory:" + productId);
        var acquired = false;
        try {
            acquired = lock.tryLock(5, 15, TimeUnit.SECONDS);
            if (!acquired) throw new InventoryConflictException("Inventory is busy; retry later");
            return action.call();
        } catch (InterruptedException exception) { Thread.currentThread().interrupt(); throw new InventoryConflictException("Inventory lock interrupted"); }
        catch (InventoryNotFoundException | InventoryConflictException exception) { throw exception; }
        catch (Exception exception) { throw new IllegalStateException(exception); }
        finally { if (acquired && lock.isHeldByCurrentThread()) lock.unlock(); }
    }
}

class InventoryNotFoundException extends RuntimeException { InventoryNotFoundException(long id) { super("Inventory not found for product " + id); } }
class InventoryConflictException extends RuntimeException { InventoryConflictException(String message) { super(message); } }

