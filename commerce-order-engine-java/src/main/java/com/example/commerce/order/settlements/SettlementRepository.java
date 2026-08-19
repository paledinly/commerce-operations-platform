package com.example.commerce.order.settlements;

import static com.example.commerce.order.settlements.SettlementModels.*;
import java.math.BigDecimal;
import java.sql.*;
import java.time.LocalDate;
import java.util.*;
import org.springframework.jdbc.core.namedparam.NamedParameterJdbcTemplate;
import org.springframework.stereotype.Repository;

@Repository
public class SettlementRepository {
    private final NamedParameterJdbcTemplate jdbc;
    public SettlementRepository(NamedParameterJdbcTemplate jdbc) { this.jdbc = jdbc; }

    public List<DailySettlement> find(LocalDate from, LocalDate to) {
        return jdbc.query("SELECT * FROM daily_sales_summaries WHERE settlement_date BETWEEN :from AND :to ORDER BY settlement_date DESC",
            Map.of("from", from, "to", to), this::map);
    }

    public void rebuild(LocalDate date) {
        var row = jdbc.queryForMap("""
            SELECT
              COALESCE(SUM(CASE WHEN transaction_type='PAYMENT' THEN amount ELSE 0 END),0) payment_amount,
              COALESCE(SUM(CASE WHEN transaction_type='REFUND' THEN amount ELSE 0 END),0) refund_amount,
              COALESCE(SUM(CASE WHEN transaction_type='PAYMENT' THEN 1 ELSE 0 END),0) payment_count,
              COALESCE(SUM(CASE WHEN transaction_type='REFUND' THEN 1 ELSE 0 END),0) refund_count
            FROM payment_transactions
            WHERE created_at>=:start AND created_at<:end
            """, Map.of("start", date.atStartOfDay(), "end", date.plusDays(1).atStartOfDay()));
        var paid = (BigDecimal) row.get("payment_amount");
        var refunded = (BigDecimal) row.get("refund_amount");
        jdbc.update("""
            INSERT INTO daily_sales_summaries(settlement_date,payment_amount,refund_amount,net_amount,payment_count,refund_count)
            VALUES(:date,:paid,:refunded,:net,:payments,:refunds)
            ON DUPLICATE KEY UPDATE payment_amount=VALUES(payment_amount),refund_amount=VALUES(refund_amount),
              net_amount=VALUES(net_amount),payment_count=VALUES(payment_count),refund_count=VALUES(refund_count),calculated_at=CURRENT_TIMESTAMP
            """, Map.of("date", date, "paid", paid, "refunded", refunded, "net", paid.subtract(refunded),
                "payments", ((Number) row.get("payment_count")).longValue(), "refunds", ((Number) row.get("refund_count")).longValue()));
    }

    private DailySettlement map(ResultSet r, int n) throws SQLException {
        return new DailySettlement(r.getDate("settlement_date").toLocalDate(), r.getBigDecimal("payment_amount"),
            r.getBigDecimal("refund_amount"), r.getBigDecimal("net_amount"), r.getLong("payment_count"),
            r.getLong("refund_count"), r.getTimestamp("calculated_at").toInstant());
    }
}
