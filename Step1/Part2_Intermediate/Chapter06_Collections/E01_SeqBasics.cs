using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter06_Collections;

/// <summary>
/// Seq 타입의 기본 사용법을 학습합니다.
///
/// 학습 목표:
/// - Seq<T>의 개념과 특성
/// - Seq 생성 방법
/// - 기본 연산 (Map, Filter, Fold 등)
/// - IEnumerable과의 차이점
///
/// 핵심 개념:
/// Seq<T>는 LanguageExt의 불변 시퀀스 타입입니다.
/// - 불변성: 수정하면 새 인스턴스 반환
/// - 지연 평가: 필요할 때만 계산
/// - 구조적 공유: 효율적인 메모리 사용
/// - 함수형 연산: Map, Bind, Fold 등 지원
/// </summary>
public static class E01_SeqBasics
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 06-E01: Seq 기본");

        // ============================================================
        // 1. Seq 생성
        // ============================================================
        MenuHelper.PrintSubHeader("1. Seq 생성");

        MenuHelper.PrintExplanation("Seq는 불변 시퀀스로, 여러 방법으로 생성할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // 직접 생성
        MenuHelper.PrintCode("var seq1 = Seq(1, 2, 3, 4, 5);");
        var seq1 = Seq(1, 2, 3, 4, 5);
        MenuHelper.PrintResult("Seq(1,2,3,4,5)", seq1);

        // 빈 Seq
        MenuHelper.PrintCode("var empty = Empty;");
        var empty = Seq<int>();
        MenuHelper.PrintResult("Empty Seq", empty);

        // 배열/리스트에서 변환
        MenuHelper.PrintCode("var fromArray = Seq(10, 20, 30);");
        var fromArray = Seq(10, 20, 30);
        MenuHelper.PrintResult("배열 → Seq", fromArray);

        // Range
        MenuHelper.PrintCode("var range = Seq(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);");
        var range = Seq(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        MenuHelper.PrintResult("Range(1, 10)", range);

        // ============================================================
        // 2. 기본 속성
        // ============================================================
        MenuHelper.PrintSubHeader("2. 기본 속성");

        var numbers = Seq(1, 2, 3, 4, 5);

        MenuHelper.PrintResult("Count", numbers.Count);
        MenuHelper.PrintResult("IsEmpty", numbers.IsEmpty);
        MenuHelper.PrintResult("Head", numbers.Head);
        MenuHelper.PrintResult("Tail", numbers.Tail);
        MenuHelper.PrintResult("Last", numbers.Last);
        MenuHelper.PrintResult("Init (마지막 제외)", numbers.Init);

        // ============================================================
        // 3. Map 연산
        // ============================================================
        MenuHelper.PrintSubHeader("3. Map 연산");

        MenuHelper.PrintExplanation("Map은 각 요소를 변환합니다.");
        MenuHelper.PrintBlankLines();

        var doubled = numbers.Map(x => x * 2);
        var strings = numbers.Map(x => $"값: {x}");

        MenuHelper.PrintResult("Map(x => x * 2)", doubled);
        MenuHelper.PrintResult("Map(x => $\"값: {x}\")", strings);

        // ============================================================
        // 4. Filter 연산
        // ============================================================
        MenuHelper.PrintSubHeader("4. Filter 연산");

        MenuHelper.PrintExplanation("Filter는 조건을 만족하는 요소만 선택합니다.");
        MenuHelper.PrintBlankLines();

        var evens = numbers.Filter(x => x % 2 == 0);
        var greaterThan3 = numbers.Filter(x => x > 3);

        MenuHelper.PrintResult("Filter(짝수)", evens);
        MenuHelper.PrintResult("Filter(>3)", greaterThan3);

        // ============================================================
        // 5. Fold 연산
        // ============================================================
        MenuHelper.PrintSubHeader("5. Fold 연산");

        MenuHelper.PrintExplanation("Fold는 시퀀스를 하나의 값으로 집계합니다.");
        MenuHelper.PrintBlankLines();

        // 합계
        var sum = numbers.Fold(0, (acc, x) => acc + x);
        MenuHelper.PrintResult("Fold (합계)", sum);

        // 곱
        var product = numbers.Fold(1, (acc, x) => acc * x);
        MenuHelper.PrintResult("Fold (곱)", product);

        // 문자열 결합
        var concat = numbers.Fold("", (acc, x) => acc + x.ToString());
        MenuHelper.PrintResult("Fold (문자열 결합)", concat);

        // ============================================================
        // 6. 요소 추가/제거
        // ============================================================
        MenuHelper.PrintSubHeader("6. 요소 추가/제거");

        MenuHelper.PrintExplanation("불변이므로 수정 시 새 Seq가 반환됩니다.");
        MenuHelper.PrintBlankLines();

        var original = Seq(1, 2, 3);

        // 앞에 추가
        var withPrepend = 0.Cons(original);
        MenuHelper.PrintResult("0.Cons(seq)", withPrepend);

        // 뒤에 추가
        var withAppend = original.Add(4);
        MenuHelper.PrintResult("seq.Add(4)", withAppend);

        // 두 Seq 결합
        var combined = original.Concat(Seq(7, 8, 9));
        MenuHelper.PrintResult("Concat", combined);

        // Skip/Take
        var skipped = numbers.Skip(2);
        var taken = numbers.Take(3);
        MenuHelper.PrintResult("Skip(2)", skipped);
        MenuHelper.PrintResult("Take(3)", taken);

        // ============================================================
        // 7. LINQ 통합
        // ============================================================
        MenuHelper.PrintSubHeader("7. LINQ 통합");

        MenuHelper.PrintExplanation("LINQ 쿼리 구문도 사용할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        var linqResult =
            from n in numbers
            where n > 2
            select n * 10;

        MenuHelper.PrintResult("LINQ (>2, *10)", linqResult);

        // SelectMany (Bind)
        var nested = Seq(Seq(1, 2), Seq(3, 4), Seq(5));
        var flattened = nested.Bind(x => x);
        MenuHelper.PrintResult("Bind (평탄화)", flattened);

        // ============================================================
        // 8. 패턴 매칭
        // ============================================================
        MenuHelper.PrintSubHeader("8. 패턴 매칭");

        MenuHelper.PrintExplanation("IsEmpty와 Head/Tail로 시퀀스를 처리합니다.");
        MenuHelper.PrintBlankLines();

        var result1 = numbers.IsEmpty
            ? "시퀀스가 비어있습니다"
            : $"첫 요소: {numbers.Head}, 나머지 개수: {numbers.Tail.Count}";
        MenuHelper.PrintResult("numbers 처리", result1);

        var result2 = empty.IsEmpty
            ? "시퀀스가 비어있습니다"
            : $"첫 요소: {empty.Head}";
        MenuHelper.PrintResult("empty 처리", result2);

        // ============================================================
        // 9. 실전 예제: 데이터 처리
        // ============================================================
        MenuHelper.PrintSubHeader("9. 실전 예제: 데이터 처리");

        var users = Seq(
            new User("Alice", 30, true),
            new User("Bob", 25, false),
            new User("Charlie", 35, true),
            new User("Diana", 28, true)
        );

        // 활성 사용자의 평균 나이
        var activeUsers = users.Filter(u => u.IsActive);
        var avgAge = activeUsers.Map(u => u.Age).Fold(0, (a, b) => a + b) / activeUsers.Count;

        MenuHelper.PrintResult("활성 사용자", activeUsers.Map(u => u.Name));
        MenuHelper.PrintResult("활성 사용자 평균 나이", avgAge);

        // 나이순 정렬
        var sorted = users.OrderBy(u => u.Age);
        MenuHelper.PrintResult("나이순", toSeq(sorted).Map(u => $"{u.Name}({u.Age})"));

        MenuHelper.PrintSuccess("Seq 기본 학습 완료!");
    }

    private record User(string Name, int Age, bool IsActive);
}
