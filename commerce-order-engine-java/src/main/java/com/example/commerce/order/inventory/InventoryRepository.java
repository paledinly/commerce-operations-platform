package com.example.commerce.order.inventory;

import static com.example.commerce.order.inventory.InventoryModels.*;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import org.springframework.jdbc.core.namedparam.NamedParameterJdbcTemplate;
import org.springframework.stereotype.Repository;

@Repository
public class InventoryRepository {
    private final NamedParameterJdbcTemplate jdbc;
    public InventoryRepository(NamedParameterJdbcTemplate jdbc) { this.jdbc = jdbc; }

    public InventoryPage findAll(int page, int pageSize) {
        var parameters = Map.of("limit", pageSize, "offset", (page - 1) * pageSize);
        var items = jdbc.query("SELECT * FROM inventories ORDER BY updated_at DESC, product_id DESC LIMIT :limit OFFSET :offset", parameters, this::mapInventory);
        var total = jdbc.queryForObject("SELECT COUNT(*) FROM inventories", Map.of(), Long.class);
        return new InventoryPage(items, page, pageSize, total == null ? 0 : total);
    }
    public Optional<Inventory> find(long productId) {
        List<Inventory> rows = jdbc.query("SELECT * FROM inventories WHERE product_id=:productId", Map.of("productId", productId), this::mapInventory);
        return rows.stream().findFirst();
    }
    public void create(long productId, long quantity) {
        jdbc.update("INSERT INTO inventories(product_id,available_quantity) VALUES(:productId,:quantity)", Map.of("productId", productId, "quantity", quantity));
        jdbc.update("INSERT INTO inventory_movements(product_id,movement_type,quantity_delta,available_after,reason) VALUES(:productId,'INITIAL',:quantity,:quantity,'Initial inventory')", Map.of("productId", productId, "quantity", quantity));
    }
    public int adjust(long productId, long delta) {
        return jdbc.update("UPDATE inventories SET available_quantity=available_quantity+:delta,version=version+1 WHERE product_id=:productId AND available_quantity+:delta>=0", Map.of("productId", productId, "delta", delta));
    }
    public void addMovement(long productId, long delta, long availableAfter, String reason) {
        jdbc.update("INSERT INTO inventory_movements(product_id,movement_type,quantity_delta,available_after,reason) VALUES(:productId,'ADJUSTMENT',:delta,:availableAfter,:reason)", Map.of("productId", productId, "delta", delta, "availableAfter", availableAfter, "reason", reason.trim()));
    }
    public List<Movement> movements(long productId) {
        return jdbc.query("SELECT * FROM inventory_movements WHERE product_id=:productId ORDER BY created_at DESC,id DESC LIMIT 100", Map.of("productId", productId), this::mapMovement);
    }
    private Inventory mapInventory(ResultSet rs, int row) throws SQLException { return new Inventory(rs.getLong("product_id"), rs.getLong("available_quantity"), rs.getLong("reserved_quantity"), rs.getLong("version"), rs.getTimestamp("created_at").toInstant(), rs.getTimestamp("updated_at").toInstant()); }
    private Movement mapMovement(ResultSet rs, int row) throws SQLException { return new Movement(rs.getLong("id"), rs.getLong("product_id"), rs.getString("movement_type"), rs.getLong("quantity_delta"), rs.getLong("available_after"), rs.getString("reason"), rs.getTimestamp("created_at").toInstant()); }
}

