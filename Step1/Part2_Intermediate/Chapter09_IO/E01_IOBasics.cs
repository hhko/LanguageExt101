using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter09_IO;

/// <summary>
/// IO 모나드의 기본 사용법을 학습합니다.
///
/// 학습 목표:
/// - IO<A>의 개념과 목적
/// - 부수 효과(Side Effect) 캡슐화
/// - IO 생성 및 실행
/// - 순수성과 참조 투명성
///
/// 핵심 개념:
/// IO<A>는 부수 효과를 수행하는 계산을 값으로 다루는 타입입니다.
/// - IO 값 자체는 순수합니다 (계산을 설명할 뿐 실행하지 않음)
/// - Run()을 호출해야 실제로 부수 효과가 발생합니다
/// - 테스트 용이성과 추론 가능성을 높여줍니다
/// </summary>
public static class E01_IOBasics
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 09-E01: IO 기본");

        // ============================================================
        // 1. IO의 필요성
        // ============================================================
        MenuHelper.PrintSubHeader("1. IO의 필요성");

        MenuHelper.PrintExplanation("순수 함수는 같은 입력에 항상 같은 출력을 반환합니다.");
        MenuHelper.PrintExplanation("하지만 현실의 프로그램은 I/O, 랜덤, 시간 등의 부수 효과가 필요합니다.");
        MenuHelper.PrintExplanation("IO 모나드는 부수 효과를 '값'으로 다룹니다.");
        MenuHelper.PrintBlankLines();

        // 순수 함수 예시
        MenuHelper.PrintCode("// 순수 함수: 항상 같은 결과");
        int PureAdd(int a, int b) => a + b;
        MenuHelper.PrintResult("PureAdd(2, 3)", PureAdd(2, 3));
        MenuHelper.PrintResult("PureAdd(2, 3)", PureAdd(2, 3));  // 항상 같음
        MenuHelper.PrintBlankLines();

        // 비순수 함수 예시
        MenuHelper.PrintCode("// 비순수 함수: DateTime.Now는 호출 시마다 다름");
        MenuHelper.PrintExplanation("IO<DateTime>으로 감싸면 '시간을 가져오는 계산'을 값으로 다룸");

        // ============================================================
        // 2. IO 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. IO 생성");

        MenuHelper.PrintExplanation("IO.lift() 또는 IO.Pure()로 IO를 생성합니다.");
        MenuHelper.PrintBlankLines();

        // 순수 값을 IO로 감싸기
        MenuHelper.PrintCode("var pureIO = IO.pure(42);");
        var pureIO = IO.pure(42);
        MenuHelper.PrintResult("IO.pure(42)", "IO<int> (아직 실행 안 됨)");

        // 부수 효과를 IO로 감싸기
        MenuHelper.PrintCode("var effectIO = IO.lift(() => DateTime.Now);");
        var effectIO = IO.lift(() => DateTime.Now);
        MenuHelper.PrintResult("IO.lift(() => DateTime.Now)", "IO<DateTime> (아직 실행 안 됨)");

        // ============================================================
        // 3. IO 실행
        // ============================================================
        MenuHelper.PrintSubHeader("3. IO 실행");

        MenuHelper.PrintExplanation("Run()을 호출해야 실제로 계산이 실행됩니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("var result = pureIO.Run();");
        var result = pureIO.Run();
        MenuHelper.PrintResult("pureIO.Run()", result);

        MenuHelper.PrintCode("var now = effectIO.Run();");
        var now = effectIO.Run();
        MenuHelper.PrintResult("effectIO.Run()", now);

        // ============================================================
        // 4. IO는 값입니다
        // ============================================================
        MenuHelper.PrintSubHeader("4. IO는 값입니다");

        MenuHelper.PrintExplanation("IO는 계산을 '설명'하는 값입니다.");
        MenuHelper.PrintExplanation("여러 번 참조해도 Run() 전까지는 실행되지 않습니다.");
        MenuHelper.PrintBlankLines();

        var printIO = IO.lift(() =>
        {
            Console.WriteLine("    [IO 실행됨!]");
            return unit;
        });

        MenuHelper.PrintCode("// printIO를 정의만 하고 아직 실행하지 않음");
        MenuHelper.PrintExplanation("위의 메시지는 아직 출력되지 않았습니다");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 이제 Run() 호출");
        printIO.Run();

        // ============================================================
        // 5. IO 합성 (Map)
        // ============================================================
        MenuHelper.PrintSubHeader("5. IO 합성 (Map)");

        MenuHelper.PrintExplanation("Map으로 IO 안의 값을 변환합니다.");
        MenuHelper.PrintBlankLines();

        var getNumber = IO.pure(10);
        var doubled = getNumber.Map(x => x * 2);

        MenuHelper.PrintCode("var doubled = IO.pure(10).Map(x => x * 2);");
        MenuHelper.PrintResult("doubled.Run()", doubled.Run());

        // 체이닝
        var chained = IO.pure(5)
            .Map(x => x * 2)
            .Map(x => x + 3)
            .Map(x => $"결과: {x}");

        MenuHelper.PrintResult("체이닝 결과", chained.Run());

        // ============================================================
        // 6. IO 합성 (Bind)
        // ============================================================
        MenuHelper.PrintSubHeader("6. IO 합성 (Bind)");

        MenuHelper.PrintExplanation("Bind로 IO를 반환하는 함수를 연결합니다.");
        MenuHelper.PrintBlankLines();

        var readName = IO.lift(() =>
        {
            Console.Write("    이름 입력 (시뮬레이션): ");
            return "테스트사용자";  // 실제로는 Console.ReadLine()
        });

        var greet = readName.Bind(name =>
            IO.lift(() =>
            {
                Console.WriteLine($"    안녕하세요, {name}님!");
                return name;
            }));

        greet.Run();

        // ============================================================
        // 7. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("7. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("LINQ로 여러 IO를 조합할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        var program =
            from a in IO.pure(10)
            from b in IO.pure(20)
            select a + b;

        var sum = program.Run();
        MenuHelper.PrintResult("LINQ 결과", sum);

        // ============================================================
        // 8. 실전 예제: 파일 읽기 시뮬레이션
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 파일 읽기 시뮬레이션");

        var readFile = (string path) => IO.lift(() =>
        {
            Console.WriteLine($"    파일 읽는 중: {path}");
            return $"파일 내용: [{path}의 데이터]";
        });

        var processFile = (string content) => IO.lift(() =>
        {
            Console.WriteLine($"    처리 중: {content}");
            return content.ToUpper();
        });

        var fileProgram =
            from content in readFile("data.txt")
            from processed in processFile(content)
            select processed;

        MenuHelper.PrintResult("파일 처리 결과", fileProgram.Run());

        MenuHelper.PrintSuccess("IO 기본 학습 완료!");
    }
}
