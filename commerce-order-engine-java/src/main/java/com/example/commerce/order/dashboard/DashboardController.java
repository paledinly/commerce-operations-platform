package com.example.commerce.order.dashboard;

import java.math.BigDecimal;
import java.util.*;
import org.springframework.jdbc.core.namedparam.NamedParameterJdbcTemplate;
import org.springframework.web.bind.annotation.*;

@RestController @RequestMapping("/internal/dashboard")
public class DashboardController {
    private final NamedParameterJdbcTemplate jdbc;
    public DashboardController(NamedParameterJdbcTemplate jdbc){this.jdbc=jdbc;}
    @GetMapping public DashboardSummary summary(){
        var totals=jdbc.queryForMap("SELECT COUNT(*) total_orders,COALESCE(SUM(CASE WHEN status IN('PAID','SHIPPED','COMPLETED') THEN total_amount ELSE 0 END),0) net_revenue FROM orders",Map.of());
        var statuses=new LinkedHashMap<String,Long>();for(var row:jdbc.queryForList("SELECT status,COUNT(*) count FROM orders GROUP BY status ORDER BY status",Map.of()))statuses.put((String)row.get("status"),((Number)row.get("count")).longValue());
        var inventory=jdbc.queryForMap("SELECT COALESCE(SUM(available_quantity),0) available,COALESCE(SUM(reserved_quantity),0) reserved,SUM(CASE WHEN available_quantity<=5 THEN 1 ELSE 0 END) low_stock FROM inventories",Map.of());
        var pending=jdbc.queryForObject("SELECT COUNT(*) FROM order_event_outbox WHERE published_at IS NULL",Map.of(),Long.class);
        return new DashboardSummary(((Number)totals.get("total_orders")).longValue(),(BigDecimal)totals.get("net_revenue"),statuses,((Number)inventory.get("available")).longValue(),((Number)inventory.get("reserved")).longValue(),((Number)inventory.get("low_stock")).longValue(),pending==null?0:pending);
    }
    public record DashboardSummary(long totalOrders,BigDecimal netRevenue,Map<String,Long> ordersByStatus,long availableQuantity,long reservedQuantity,long lowStockProducts,long pendingEvents){}
}
