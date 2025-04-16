# Sparta_TextRPG
내일 배움캠프 C#기초강의 개인 프로젝트인 TextRPG입니다

주요 기능

플레이어 상태 확인: 직업, 레벨, 능력치, 골드 확인

상점 시스템: 아이템 구매 / 판매 가능

인벤토리 관리: 아이템 보유 확인 및 장착/해제

던전 탐험: 난이도 별 던전 입장, 보상 획득

휴식 시스템: 골드를 사용해 체력 회복

레벨 시스템: 던전 클리어 수에 따라 자동 레벨업

데이터 저장/불러오기: Newtonsoft.Json을 이용한 JSON 기반 세이브 시스템

아키텍처 설계

상태 기반 액션 시스템

IAction 인터페이스와 ActionBase 추상 클래스를 중심으로 상태 전이 설계

모든 메뉴/행동은 IAction 구현 클래스로 정의

subActionMap을 통해 유저 입력 → 다음 상태 전환 구조

SOLID 원칙 적용

SRP: 각 매니저 클래스(Inventory, Equipment 등)는 하나의 책임만 가짐

OCP: 새로운 행동은 클래스만 추가하면 동작 가능

LSP/ISP: IAction, IHasSubActions로 분리

DIP: 일부 static 매니저 구조 → 향후 DI 리팩토링 가능

테이블 기반 구조

ItemTable, JobTable, DungeonTable 등 데이터 테이블 분리

Dictionary 기반 ID → 객체 참조


개발 의도

이 프로젝트는 객체지향 설계(SOLID 원칙)를 기반으로 한 텍스트 RPG 구조 설계 학습 및 포트폴리오용 구현을 목적으로 만들어졌습니다. 전체 구조는 확장성과 유지보수성을 고려해 설계되었으며, 콘솔 환경에서도 깔끔한 사용자 경험을 제공하는 데 집중했습니다.
