package com.example.commerce.order.settlements;
import java.math.BigDecimal;import java.time.*;import java.util.List;
public final class SettlementModels{private SettlementModels(){}public record DailySettlement(LocalDate settlementDate,BigDecimal paymentAmount,BigDecimal refundAmount,BigDecimal netAmount,long paymentCount,long refundCount,Instant calculatedAt){}public record SettlementPage(List<DailySettlement> items,LocalDate from,LocalDate to){}public record RebuildResult(LocalDate from,LocalDate to,int rebuiltDays){} }
