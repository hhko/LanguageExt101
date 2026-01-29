using LanguageExt101.Common;
using LanguageExt101.Part1_Fundamentals.Chapter01_Introduction;
using LanguageExt101.Part1_Fundamentals.Chapter02_Option;
using LanguageExt101.Part1_Fundamentals.Chapter03_Either;
using LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;
using LanguageExt101.Part1_Fundamentals.Chapter05_Prelude;
using LanguageExt101.Part2_Intermediate.Chapter06_Collections;
using LanguageExt101.Part2_Intermediate.Chapter07_Validation;
using LanguageExt101.Part2_Intermediate.Chapter08_Guard;
using LanguageExt101.Part2_Intermediate.Chapter09_IO;
using LanguageExt101.Part2_Intermediate.Chapter10_Eff;
using LanguageExt101.Part2_Intermediate.Chapter11_Transformers;
using LanguageExt101.Part3_Advanced.Chapter12_Traits;
using LanguageExt101.Part3_Advanced.Chapter13_HigherKinded;
using LanguageExt101.Part3_Advanced.Chapter14_RealWorld;

/// <summary>
/// LanguageExt101 - LanguageExt 5.0 학습 프로젝트
///
/// 이 프로젝트는 LanguageExt 라이브러리의 핵심 개념들을
/// 기초부터 고급까지 단계별로 학습할 수 있도록 구성되어 있습니다.
///
/// 구성:
/// - Part 1: 기초 (Fundamentals) - Option, Either, Fin, Error, Prelude 함수들
/// - Part 2: 중급 (Intermediate) - Collections, Validation, Guard, IO, Eff, Transformers
/// - Part 3: 고급 (Advanced) - Traits 시스템, K<F,A> 패턴, 실무 적용
/// </summary>
Console.OutputEncoding = System.Text.Encoding.UTF8;

while (true)
{
    var mainChoice = MenuHelper.ShowMenu(
        "LanguageExt101 - 함수형 프로그래밍 학습",
        (1, "Part 1: 기초 (Fundamentals)"),
        (2, "Part 2: 중급 (Intermediate)"),
        (3, "Part 3: 고급 (Advanced)")
    );

    switch (mainChoice)
    {
        case 0:
            Console.WriteLine("\n  학습을 종료합니다. 수고하셨습니다!\n");
            return;
        case 1:
            RunPart1Menu();
            break;
        case 2:
            RunPart2Menu();
            break;
        case 3:
            RunPart3Menu();
            break;
        default:
            Console.WriteLine("\n  잘못된 선택입니다. 다시 선택해주세요.");
            break;
    }
}

// ============================================================
// Part 1: 기초 (Fundamentals) 메뉴
// ============================================================
void RunPart1Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Part 1: 기초 (Fundamentals)",
            (1, "Chapter 01: 소개 및 Prelude 기본"),
            (2, "Chapter 02: Option - null 안전성"),
            (3, "Chapter 03: Either - 명시적 오류 처리"),
            (4, "Chapter 04: Fin과 Error"),
            (5, "Chapter 05: Prelude 함수들")
        );

        switch (choice)
        {
            case 0: return;
            case 1: RunChapter01Menu(); break;
            case 2: RunChapter02Menu(); break;
            case 3: RunChapter03Menu(); break;
            case 4: RunChapter04Menu(); break;
            case 5: RunChapter05Menu(); break;
            default:
                Console.WriteLine("\n  잘못된 선택입니다.");
                break;
        }
    }
}

// ============================================================
// Part 2: 중급 (Intermediate) 메뉴
// ============================================================
void RunPart2Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Part 2: 중급 (Intermediate)",
            (1, "Chapter 06: 불변 컬렉션"),
            (2, "Chapter 07: Validation"),
            (3, "Chapter 08: guard와 조건부 모나드"),
            (4, "Chapter 09: IO 모나드"),
            (5, "Chapter 10: Eff 모나드"),
            (6, "Chapter 11: 모나드 트랜스포머")
        );

        switch (choice)
        {
            case 0: return;
            case 1: RunChapter06Menu(); break;
            case 2: RunChapter07Menu(); break;
            case 3: RunChapter08Menu(); break;
            case 4: RunChapter09Menu(); break;
            case 5: RunChapter10Menu(); break;
            case 6: RunChapter11Menu(); break;
            default:
                Console.WriteLine("\n  잘못된 선택입니다.");
                break;
        }
    }
}

// ============================================================
// Part 3: 고급 (Advanced) 메뉴
// ============================================================
void RunPart3Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Part 3: 고급 (Advanced)",
            (1, "Chapter 12: Traits 시스템"),
            (2, "Chapter 13: K<F, A> 패턴"),
            (3, "Chapter 14: 실무 적용")
        );

        switch (choice)
        {
            case 0: return;
            case 1: RunChapter12Menu(); break;
            case 2: RunChapter13Menu(); break;
            case 3: RunChapter14Menu(); break;
            default:
                Console.WriteLine("\n  잘못된 선택입니다.");
                break;
        }
    }
}

// ============================================================
// Chapter 메뉴들
// ============================================================

void RunChapter01Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 01: 소개 및 Prelude 기본",
            (1, "E01: Prelude 기본 사용법"),
            (2, "E02: unit, identity, constant")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_PreludeBasics.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_UnitAndIdentity.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter02Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 02: Option - null 안전성",
            (1, "E01: Option 생성"),
            (2, "E02: Match와 Map"),
            (3, "E03: Bind와 LINQ"),
            (4, "Exercise01: 안전한 연산 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_CreatingOptions.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_MatchAndMap.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_BindAndLinq.Run(); MenuHelper.WaitForKey(); break;
            case 4: Exercise01_SafeOperations.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter03Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 03: Either - 명시적 오류 처리",
            (1, "E01: Left/Right 기본"),
            (2, "E02: Either 체이닝"),
            (3, "Exercise02: 에러 처리 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_LeftRight.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_EitherChaining.Run(); MenuHelper.WaitForKey(); break;
            case 3: Exercise02_ErrorHandling.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter04Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 04: Fin과 Error",
            (1, "E01: Error 타입 종류"),
            (2, "E02: Fin 기본 사용"),
            (3, "E03: 에러 결합"),
            (4, "Exercise03: API 결과 처리 (실습)"),
            (5, "E04: Try 기본"),
            (6, "E05: Try 합성"),
            (7, "E06: 실무 Try 활용"),
            (8, "Exercise04: 예외 안전 파이프라인 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_ErrorTypes.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_FinBasics.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_ErrorCombining.Run(); MenuHelper.WaitForKey(); break;
            case 4: Exercise03_ApiResults.Run(); MenuHelper.WaitForKey(); break;
            case 5: E04_TryBasics.Run(); MenuHelper.WaitForKey(); break;
            case 6: E05_TryComposition.Run(); MenuHelper.WaitForKey(); break;
            case 7: E06_TryRealWorld.Run(); MenuHelper.WaitForKey(); break;
            case 8: Exercise04_TryPipeline.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter05Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 05: Prelude 함수들",
            (1, "E01: 커링과 부분 적용"),
            (2, "E02: 함수 합성"),
            (3, "E03: 메모이제이션")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_CurryingPartial.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_Composition.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_Memoization.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter06Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 06: 불변 컬렉션",
            (1, "E01: Seq 기본"),
            (2, "E02: Map과 Set"),
            (3, "Exercise04: 데이터 처리 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_SeqBasics.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_MapAndSet.Run(); MenuHelper.WaitForKey(); break;
            case 3: Exercise04_DataProcessing.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter07Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 07: Validation",
            (1, "E01: Validation 기본"),
            (2, "E02: Applicative 스타일"),
            (3, "E03: 실무 검증 예제"),
            (4, "Exercise05: 폼 검증 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_ValidationBasics.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_ApplicativeStyle.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_RealWorldValidation.Run(); MenuHelper.WaitForKey(); break;
            case 4: Exercise05_FormValidation.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter08Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 08: guard와 조건부 모나드",
            (1, "E01: guard 기본"),
            (2, "E02: when과 unless"),
            (3, "E03: 파이프라인에서 guard")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_GuardBasics.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_WhenUnless.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_GuardInPipelines.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter09Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 09: IO 모나드",
            (1, "E01: IO 기본"),
            (2, "E02: IO 합성"),
            (3, "E03: 리소스 관리"),
            (4, "Exercise06: 파일 작업 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_IOBasics.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_IOComposition.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_ResourceManagement.Run(); MenuHelper.WaitForKey(); break;
            case 4: Exercise06_FileOperations.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter10Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 10: Eff 모나드",
            (1, "E01: Eff 기본"),
            (2, "E02: Eff<RT, A>와 런타임"),
            (3, "E03: retry와 Schedule"),
            (4, "Exercise07: API 클라이언트 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_EffBasics.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_EffWithRuntime.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_RetryAndSchedule.Run(); MenuHelper.WaitForKey(); break;
            case 4: Exercise07_ApiClient.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter11Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 11: 모나드 트랜스포머",
            (1, "E01: OptionT"),
            (2, "E02: EitherT"),
            (3, "E03: FinT"),
            (4, "Exercise08: 비동기 파이프라인 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_OptionT.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_EitherT.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_FinT.Run(); MenuHelper.WaitForKey(); break;
            case 4: Exercise08_AsyncPipeline.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter12Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 12: Traits 시스템",
            (1, "E01: Traits 시스템 개요"),
            (2, "E02: Functor와 Applicative"),
            (3, "E03: Monad와 Alternative"),
            (4, "E04: Foldable과 Traversable"),
            (5, "Exercise09: 제네릭 함수 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_TraitsOverview.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_FunctorApplicative.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_MonadAlternative.Run(); MenuHelper.WaitForKey(); break;
            case 4: E04_FoldableTraversable.Run(); MenuHelper.WaitForKey(); break;
            case 5: Exercise09_GenericFunctions.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter13Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 13: K<F, A> 패턴",
            (1, "E01: K<F, A> 기본"),
            (2, "E02: As() 변환"),
            (3, "E03: 범용 알고리즘"),
            (4, "Exercise10: 추상 파이프라인 (실습)")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_KBasics.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_AsExtension.Run(); MenuHelper.WaitForKey(); break;
            case 3: E03_GenericAlgorithms.Run(); MenuHelper.WaitForKey(); break;
            case 4: Exercise10_AbstractPipeline.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}

void RunChapter14Menu()
{
    while (true)
    {
        var choice = MenuHelper.ShowMenu(
            "Chapter 14: 실무 적용",
            (1, "E01: Before/After 리팩토링"),
            (2, "E02: 도메인 모델링"),
            (3, "최종 프로젝트: 미니 쇼핑몰")
        );

        switch (choice)
        {
            case 0: return;
            case 1: E01_BeforeAfter.Run(); MenuHelper.WaitForKey(); break;
            case 2: E02_DomainModeling.Run(); MenuHelper.WaitForKey(); break;
            case 3: FinalProject.Run(); MenuHelper.WaitForKey(); break;
            default: Console.WriteLine("\n  잘못된 선택입니다."); break;
        }
    }
}
