# Commerce Operations Platform 작업 규칙

## 목적과 구조
이 디렉터리는 Git 저장소가 아닌 로컬 통합 워크스페이스다. `commerce-operations-dotnet`은 운영자 API, `commerce-order-engine-java`는 주문 엔진, `commerce-operations-react`는 운영 UI, `commerce-platform-infra`는 로컬 인프라를 소유한다.

## 기술과 금지 사항
- .NET 8/C# 12, ASP.NET Core, Dapper, FluentValidation, JWT, Serilog, Polly, xUnit
- Java 21, Spring Boot 3, Gradle, Spring JDBC, Flyway, Redis/Redisson, RabbitMQ, Batch, Resilience4j, JUnit 5 (JPA 금지)
- React/TypeScript/Vite, TanStack Query, Zustand, Axios, React Hook Form, Zod, MUI, Vitest
- CI 파일 생성, 유료 서비스 의존, 과도한 설계를 금지한다.

## 아키텍처와 데이터 소유권
- 서비스 간 연동은 REST API를 사용하며 다른 서비스의 테이블을 직접 읽거나 수정하지 않는다.
- C#은 `commerce_operations`, Java는 `commerce_order_engine` 데이터베이스만 소유한다.
- C# 마이그레이션은 명시적 SQL, Java 마이그레이션은 Flyway를 사용한다.
- 비밀값은 코드에 넣지 않고 `.env.local`에서 주입한다.
- 운영자 인증은 C# API가 소유하며 비밀번호 원문을 저장하지 않는다. React는 `/api` reverse proxy로만 인증 API를 호출한다.

## 개발 및 검증
- 기존 파일을 먼저 읽고 최소 변경한다. 실행하지 않은 테스트를 성공으로 보고하지 않는다.
- 단위 테스트는 외부 인프라 없이 실행하고, 통합 테스트는 격리된 컨테이너를 사용한다.
- 전체 검증: `scripts/verify-all.ps1` 또는 `scripts/verify-all.sh`
- 통합 실행: `docker compose -f commerce-platform-infra/compose.yml --env-file .env.local up --build`
