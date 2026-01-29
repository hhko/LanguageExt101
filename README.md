# LanguageExt101

**LanguageExt 5.0 라이브러리 학습을 위한 튜토리얼 프로젝트**

C#에서 함수형 프로그래밍을 구현하는 [LanguageExt](https://github.com/louthy/language-ext) 라이브러리의 핵심 개념을 기초부터 고급까지 단계별로 학습할 수 있습니다.

## 빠른 시작

### 필수 요구사항

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) 이상

### 설치 및 실행

```bash
# 저장소 클론
git clone https://github.com/hhko/LanguageExt101.git
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
│   │       ├── E01_OptionT.cs
│   │       ├── E02_EitherT.cs
│   │       ├── E03_FinT.cs
│   │       ├── E04_StateT.cs         # StateT 모나드 트랜스포머
│   │       └── E05_CompositeStack.cs # 복합 트랜스포머 스택
│   └── Part3_Advanced/               # 고급 (3개 Chapter)
│       ├── Chapter12_Traits/
│       │   ├── E01_TraitsOverview.cs
│       │   ├── E02_FunctorApplicative.cs
│       │   ├── E03_MonadAlternative.cs
│       │   ├── E04_FoldableTraversable.cs
│       │   ├── E05_Deriving.cs       # Deriving 패턴
│       │   └── E06_MonadIOAndStateful.cs  # MonadIO/Stateful 트레잇
│       ├── Chapter13_HigherKinded/
│       └── Chapter14_RealWorld/
│           └── FinalProject.cs       # Pontoon 카드 게임
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
| 11 | 모나드 트랜스포머 | OptionT, EitherT, FinT, **StateT**, **복합 스택** |

### Part 3: 고급 (Advanced)

| Chapter | 주제 | 내용 |
|---------|------|------|
| 12 | Traits 시스템 | Functor, Applicative, Monad, Alternative, Foldable, Traversable, **Deriving**, **MonadIO/Stateful** |
| 13 | K<F, A> 패턴 | Higher-Kinded Types, As() 변환, 범용 알고리즘 |
| 14 | 실무 적용 | Before/After 리팩토링, 도메인 모델링, **Pontoon 카드 게임** |

## 최종 프로젝트 학습 경로

FinalProject.cs(Pontoon 카드 게임)는 LanguageExt의 고급 기능을 종합적으로 활용합니다.

### 핵심 개념

```csharp
// Game 모나드 = Monad Transformer 스택
Game<A> = StateT<GameState, OptionT<IO>, A>
```

- **StateT**: 게임 상태(덱, 플레이어) 관리
- **OptionT**: 게임 취소/종료 처리 (None = 게임 끝)
- **IO**: 콘솔 입출력 등 부수효과

### 권장 선행 학습

| 순서 | 내용 | 위치 |
|------|------|------|
| 1 | Option, Either 기본 | Part 1 Chapter 2-3 |
| 2 | IO 모나드 | Part 2 Chapter 9 |
| 3 | OptionT 트랜스포머 | Part 2 Chapter 11 E01 |
| 4 | **StateT 트랜스포머** | Part 2 Chapter 11 E04 |
| 5 | **복합 트랜스포머 스택** | Part 2 Chapter 11 E05 |
| 6 | Traits 시스템 개요 | Part 3 Chapter 12 E01 |
| 7 | **Deriving 패턴** | Part 3 Chapter 12 E05 |
| 8 | **MonadIO/Stateful** | Part 3 Chapter 12 E06 |
| 9 | K<F, A> 패턴 | Part 3 Chapter 13 |
| 10 | FinalProject | Part 3 Chapter 14 |

### FinalProject에서 사용되는 주요 기법

| 기법 | 위치 (Line) | 설명 |
|------|-------------|------|
| StateT | 312 | 게임 상태 관리의 핵심 |
| Deriving.Monad | 375 | 커스텀 모나드에 트레잇 자동 구현 |
| Deriving.Stateful | 376 | 상태 연산(get/put/modify) 자동 구현 |
| MonadIO | 377-390 | IO를 Game 모나드로 리프팅 |
| TraverseM | 672-673 | 플레이어 순회 (순차적 효과) |
| >>> 연산자 | 898-900 | 순차 실행 (결과 무시) |
| IgnoreF | 667 | 결과값 무시, 효과만 유지 |

## 학습 가이드

### 권장 학습 순서

1. **Part 1**을 순서대로 학습하세요. Option과 Either는 함수형 프로그래밍의 기본입니다.
2. **Part 2**에서는 실무에서 자주 사용하는 패턴들을 다룹니다. IO, Eff, 그리고 **트랜스포머(특히 StateT)**를 중점적으로 학습하세요.
3. **Part 3**은 LanguageExt의 고급 기능입니다. Traits, **Deriving**, Higher-Kinded Types를 이해하면 라이브러리를 더 깊이 활용할 수 있습니다.
4. **FinalProject**는 모든 개념을 통합한 실전 예제입니다. 위의 선행 학습 경로를 따라 준비한 후 도전하세요.

### 예제 파일 명명 규칙

- `E01_`, `E02_`, ... : 순차적 학습 예제
- `Exercise01_`, `Exercise02_`, ... : 직접 실습해보는 연습 문제

## 기술 스택

| 항목 | 버전 |
|------|------|
| .NET SDK | 10.0 |
| LanguageExt.Core | 5.0.0-beta-77 |
| LanguageExt.Sys | 5.0.0-beta-77 |
| C# | default (latest) |
