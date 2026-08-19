# Phase 12 운영 안정화

## 요청 추적과 보안

- 모든 C#·Java 응답에 `X-Correlation-ID`를 반환한다.
- 클라이언트가 안전한 형식의 ID를 보내면 유지하고, 없거나 유효하지 않으면 새 ID를 생성한다.
- C#이 Java를 호출할 때 같은 ID를 전달해 서비스 간 로그를 연결한다.
- 응답에 `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`를 적용한다.
- 로그인은 분당 10회로 제한하며 초과 요청은 `429 Too Many Requests`다.

## Health Check

- C# `/health/live`: 프로세스가 요청을 받을 수 있는지 확인
- C# `/health/ready`: Operations MySQL과 Java 주문 엔진 Actuator 확인
- Java `/actuator/health`: MySQL, Redis, RabbitMQ를 포함한 Spring Boot 준비 상태
- React `/health`: Nginx 정적 서비스 확인

Docker Compose는 단순 프로세스 응답 대신 Readiness 주소를 사용한다. 로컬 전체 확인은 `scripts/check-local-health.ps1` 또는 `.sh`를 실행한다.

## 조회 재시도 원칙

C#에서 Java로 보내는 `GET` 요청만 네트워크 오류, `408`, `429`, `5xx`에 대해 100ms, 200ms 간격으로 최대 3회 시도한다. 주문 생성·결제·환불·배송 같은 변경 요청은 중복 실행 위험이 있으므로 자동 재시도하지 않는다.

## 장애 확인 순서

1. `scripts/check-local-health.ps1`로 실패 계층을 확인한다.
2. `docker compose ... ps`로 컨테이너 상태를 확인한다.
3. `docker compose ... logs --tail 200 <service>`로 같은 `X-Correlation-ID`를 검색한다.
4. MySQL, Redis, RabbitMQ가 정상인데 C# Readiness만 실패하면 Java Actuator를 확인한다.
5. 복구 후 Health 스크립트를 다시 실행한다.
