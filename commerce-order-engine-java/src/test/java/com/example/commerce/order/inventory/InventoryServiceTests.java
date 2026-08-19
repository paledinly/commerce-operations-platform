package com.example.commerce.order.inventory;

import static com.example.commerce.order.inventory.InventoryModels.*;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.mockito.ArgumentMatchers.anyLong;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.time.Instant;
import java.util.Optional;
import java.util.concurrent.TimeUnit;
import org.junit.jupiter.api.Test;
import org.redisson.api.RLock;
import org.redisson.api.RedissonClient;

class InventoryServiceTests {
    @Test
    void adjustment_cannot_make_available_quantity_negative() throws InterruptedException {
        var repository = mock(InventoryRepository.class);
        var redisson = mock(RedissonClient.class);
        var lock = mock(RLock.class);
        var inventory = new Inventory(10, 2, 0, 0, Instant.now(), Instant.now());

        when(redisson.getLock("inventory:10")).thenReturn(lock);
        when(lock.tryLock(5, 15, TimeUnit.SECONDS)).thenReturn(true);
        when(lock.isHeldByCurrentThread()).thenReturn(true);
        when(repository.find(10)).thenReturn(Optional.of(inventory));
        when(repository.adjust(anyLong(), anyLong())).thenReturn(0);

        var service = new InventoryService(repository, redisson);

        assertThrows(InventoryConflictException.class,
            () -> service.adjust(10, new AdjustInventoryRequest(-3, "출고")));
    }
}
