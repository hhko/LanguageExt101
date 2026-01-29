# LanguageExt101

**LanguageExt 5.0 라이브러리 학습을 위한 튜토리얼 프로젝트**

C#에서 함수형 프로그래밍을 구현하는 [LanguageExt](https://github.com/louthy/language-ext) 라이브러리의 핵심 개념을 기초부터 고급까지 단계별로 학습할 수 있습니다.

## 빠른 시작

### 필수 요구사항

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) 이상

### 설치 및 실행

```bash
# 저장소 클론
git clone https://github.com/example/LanguageExt101.git
cd LanguageExt101

# 빌드
dotnet build

# 실행
dotnet run --project Step1/LanguageExt101.csproj
```

실행하면 대화형 콘솔 메뉴가 나타나며, Part와 Chapter를 선택하여 각 예제를 실행할 수 있습니다.

## 프로젝트 구조

```
LanguageExt101/
├── Step1/
│   ├── Program.cs                    # 메인 진입점 (대화형 메뉴)
│   ├── LanguageExt101.csproj         # 프로젝트 설정
│   ├── Common/                       # 공통 유틸리티
│   ├── Part1_Fundamentals/           # 기초 (5개 Chapter)
│   │   ├── Chapter01_Introduction/
│   │   ├── Chapter02_Option/
│   │   ├── Chapter03_Either/
│   │   ├── Chapter04_FinAndError/
│   │   └── Chapter05_Prelude/
│   ├── Part2_Intermediate/           # 중급 (6개 Chapter)
│   │   ├── Chapter06_Collections/
│   │   ├── Chapter07_Validation/
│   │   ├── Chapter08_Guard/
│   │   ├── Chapter09_IO/
│   │   ├── Chapter10_Eff/
│   │   └── Chapter11_Transformers/
│   └── Part3_Advanced/               # 고급 (3개 Chapter)
│       ├── Chapter12_Traits/
│       ├── Chapter13_HigherKinded/
│       └── Chapter14_RealWorld/
└── README.md
```

## 커리큘럼

### Part 1: 기초 (Fundamentals)

| Chapter | 주제 | 내용 |
|---------|------|------|
| 01 | 소개 및 Prelude 기본 | Prelude 사용법, unit, identity, constant |
| 02 | Option | null 안전성, Some/None, Match, Map, Bind, LINQ |
| 03 | Either | 명시적 오류 처리, Left/Right, 체이닝 |
| 04 | Fin과 Error | Error 타입, Fin 모나드, Try, 에러 결합 |
| 05 | Prelude 함수들 | 커링, 부분 적용, 함수 합성, 메모이제이션 |

### Part 2: 중급 (Intermediate)

| Chapter | 주제 | 내용 |
|---------|------|------|
| 06 | 불변 컬렉션 | Seq, Map, Set 기본 및 데이터 처리 |
| 07 | Validation | Validation 기본, Applicative 스타일, 폼 검증 |
| 08 | Guard | guard 기본, when/unless, 파이프라인 적용 |
| 09 | IO 모나드 | IO 기본, 합성, 리소스 관리, 병렬 처리 |
| 10 | Eff 모나드 | Eff 기본, 런타임, retry, Schedule |
| 11 | 모나드 트랜스포머 | OptionT, EitherT, FinT, 비동기 파이프라인 |

### Part 3: 고급 (Advanced)

| Chapter | 주제 | 내용 |
|---------|------|------|
| 12 | Traits 시스템 | Functor, Applicative, Monad, Alternative, Foldable, Traversable |
| 13 | K<F, A> 패턴 | Higher-Kinded Types, As() 변환, 범용 알고리즘 |
| 14 | 실무 적용 | Before/After 리팩토링, 도메인 모델링, 미니 쇼핑몰 프로젝트 |

## 학습 가이드

### 권장 학습 순서

1. **Part 1**을 순서대로 학습하세요. Option과 Either는 함수형 프로그래밍의 기본입니다.
2. **Part 2**에서는 실무에서 자주 사용하는 패턴들을 다룹니다. Validation과 Eff를 중점적으로 학습하세요.
3. **Part 3**은 LanguageExt의 고급 기능입니다. Traits와 Higher-Kinded Types를 이해하면 라이브러리를 더 깊이 활용할 수 있습니다.

### 예제 파일 명명 규칙

- `E01_`, `E02_`, ... : 순차적 학습 예제
- `Exercise01_`, `Exercise02_`, ... : 직접 실습해보는 연습 문제

### 학습 팁

- 각 예제 파일에는 한국어 주석으로 상세한 설명이 포함되어 있습니다.
- 예제 코드를 직접 수정하고 실행해보며 학습하세요.
- Exercise 파일은 TODO 주석을 따라 직접 구현해보세요.

## 기술 스택

| 항목 | 버전 |
|------|------|
| .NET SDK | 10.0 |
| LanguageExt.Core | 5.0.0-beta-77 |
| LanguageExt.Sys | 5.0.0-beta-77 |
| C# | default (latest) |
