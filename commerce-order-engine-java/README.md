# Commerce Order Engine

Java 21/Spring Boot 주문 엔진이다. Spring JDBC와 MySQL로 재고·주문·결제·배송·일별 정산을 처리하고, Redis·Redisson으로 상품별 동시성을 제어하며 Transactional Outbox와 RabbitMQ로 주문 이벤트를 발행한다. JPA는 사용하지 않는다.

`./gradlew clean build`, `./gradlew test`, `./gradlew bootRun`으로 검증·실행한다. 상태 확인은 `/actuator/health`, Swagger는 `/swagger-ui/index.html`이다. 포함된 Gradle 실행기는 버전 일관성을 위해 Docker의 Gradle 8.14/JDK 21 이미지를 사용하며 Flyway가 시작 시 마이그레이션을 적용한다. 내부 업무 API는 C# Operations API가 내부 API 키와 함께 호출한다.

전체 기술 설명은 [기술 스택과 기능별 적용](../docs/technology-stack.md)을 참고한다.
