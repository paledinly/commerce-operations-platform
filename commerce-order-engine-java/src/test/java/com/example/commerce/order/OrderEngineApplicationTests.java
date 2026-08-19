package com.example.commerce.order;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.assertEquals;
class OrderEngineApplicationTests { @Test void scaffoldLoads() { assertEquals("UP", new HealthController().health().get("status")); } }

