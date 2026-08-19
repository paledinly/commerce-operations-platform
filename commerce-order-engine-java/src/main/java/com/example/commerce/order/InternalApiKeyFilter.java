package com.example.commerce.order;

import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

@Component
public class InternalApiKeyFilter extends OncePerRequestFilter {
    private final byte[] expected;
    public InternalApiKeyFilter(@Value("${commerce.internal-api-key}") String key) { this.expected = key.getBytes(StandardCharsets.UTF_8); }
    @Override protected boolean shouldNotFilter(HttpServletRequest request) { return !request.getRequestURI().startsWith("/internal/"); }
    @Override protected void doFilterInternal(HttpServletRequest request, HttpServletResponse response, FilterChain chain) throws ServletException, IOException {
        var supplied = request.getHeader("X-Internal-Api-Key");
        if (supplied == null || !MessageDigest.isEqual(expected, supplied.getBytes(StandardCharsets.UTF_8))) { response.sendError(HttpServletResponse.SC_UNAUTHORIZED); return; }
        chain.doFilter(request, response);
    }
}
