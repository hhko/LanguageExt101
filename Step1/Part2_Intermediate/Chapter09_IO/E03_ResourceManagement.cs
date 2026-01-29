using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter09_IO;

/// <summary>
/// IO를 사용한 리소스 관리를 학습합니다.
///
/// 학습 목표:
/// - Bracket 패턴 (Acquire-Use-Release)
/// - Finally로 정리 작업 보장
/// - Try/Catch로 에러 처리
/// - 중첩된 리소스 관리
///
/// 핵심 개념:
/// 리소스 관리는 획득한 리소스를 사용 후 반드시 해제하는 것입니다.
/// Bracket 패턴은 예외가 발생해도 리소스 해제를 보장합니다.
/// </summary>
public static class E03_ResourceManagement
{
    // 헬퍼 타입: 시뮬레이션된 리소스들
    private record SimulatedFile(string Path, int Handle)
    {
        public bool IsOpen { get; set; } = true;
    }

    private record DbConnection(int Id)
    {
        public bool IsConnected { get; set; } = true;
    }

    private record DbTransaction(DbConnection Conn, string TxId)
    {
        public bool IsActive { get; set; } = true;
    }

    // 파일 핸들 카운터 (리소스 추적용)
    private static int _fileHandleCounter;
    private static int _connectionCounter;

    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 09-E03: 리소스 관리");

        // ============================================================
        // 1. 리소스 관리의 필요성
        // ============================================================
        MenuHelper.PrintSubHeader("1. 리소스 관리의 필요성");

        MenuHelper.PrintExplanation("파일, DB 연결, 네트워크 소켓 등은 사용 후 반드시 해제해야 합니다.");
        MenuHelper.PrintExplanation("예외가 발생해도 리소스 해제가 보장되어야 합니다.");
        MenuHelper.PrintExplanation("C#의 using 문이나 try-finally와 유사하지만, IO로 합성 가능합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 전통적인 패턴
try {
    var file = OpenFile(path);
    ProcessFile(file);
} finally {
    CloseFile(file);  // 예외와 관계없이 항상 실행
}");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("IO의 Bracket 패턴은 이를 함수형으로 표현합니다.");

        // ============================================================
        // 2. Bracket 기초 - Acquire-Use-Release
        // ============================================================
        MenuHelper.PrintSubHeader("2. Bracket 기초 - Acquire-Use-Release");

        MenuHelper.PrintExplanation("Bracket은 3단계로 구성됩니다:");
        MenuHelper.PrintExplanation("1. Acquire: 리소스 획득");
        MenuHelper.PrintExplanation("2. Use: 리소스 사용");
        MenuHelper.PrintExplanation("3. Release: 리소스 해제 (항상 실행)");
        MenuHelper.PrintBlankLines();

        IO<SimulatedFile> OpenFile(string path) => IO.lift(() =>
        {
            var handle = Interlocked.Increment(ref _fileHandleCounter);
            Console.WriteLine($"    [파일 열기] {path} (핸들: {handle})");
            return new SimulatedFile(path, handle);
        });

        IO<Unit> CloseFile(SimulatedFile file) => IO.lift(() =>
        {
            file.IsOpen = false;
            Console.WriteLine($"    [파일 닫기] {file.Path} (핸들: {file.Handle})");
            return unit;
        });

        IO<string> ReadFile(SimulatedFile file) => IO.lift(() =>
        {
            Console.WriteLine($"    [파일 읽기] {file.Path}");
            return $"{file.Path}의 내용";
        });

        MenuHelper.PrintCode(@"var bracketExample = OpenFile(""data.txt"")
    .Bracket(
        Use: file => ReadFile(file),
        Fin: file => CloseFile(file)
    );");

        var bracketExample = OpenFile("data.txt")
            .Bracket(
                Use: file => ReadFile(file),
                Fin: file => CloseFile(file)
            );

        var content = bracketExample.Run();
        MenuHelper.PrintResult("읽은 내용", content);

        // ============================================================
        // 3. Finally 패턴 - 항상 실행되는 정리
        // ============================================================
        MenuHelper.PrintSubHeader("3. Finally 패턴 - 항상 실행되는 정리");

        MenuHelper.PrintExplanation("Finally는 성공/실패와 관계없이 정리 작업을 실행합니다.");
        MenuHelper.PrintBlankLines();

        var counter = 0;

        IO<int> IncrementCounter() => IO.lift(() =>
        {
            counter++;
            Console.WriteLine($"    카운터 증가: {counter}");
            return counter;
        });

        IO<Unit> LogCompletion() => IO.lift(() =>
        {
            Console.WriteLine($"    [Finally] 작업 완료, 최종 카운터: {counter}");
            return unit;
        });

        MenuHelper.PrintCode("var withFinally = IncrementCounter().Finally(LogCompletion());");

        var withFinally = IncrementCounter().Finally(LogCompletion());
        withFinally.Run();

        // Finally는 예외에서도 실행됨
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("예외가 발생해도 Finally는 실행됩니다:");

        IO<int> FailingOperation() => IO.lift<int>(() =>
        {
            Console.WriteLine("    작업 실행 중...");
            throw new InvalidOperationException("작업 실패!");
        });

        var withFinallyFailing = FailingOperation()
            .Finally(IO.lift(() =>
            {
                Console.WriteLine("    [Finally] 정리 작업 실행됨 (예외 후)");
                return unit;
            }));

        try
        {
            withFinallyFailing.Run();
        }
        catch (Exception ex)
        {
            MenuHelper.PrintResult("예외 발생", ex.Message);
        }

        // ============================================================
        // 4. Try/Catch - 예외를 Fin으로 변환
        // ============================================================
        MenuHelper.PrintSubHeader("4. Try/Catch - 예외를 Fin으로 변환");

        MenuHelper.PrintExplanation("Try()는 예외를 Fin<A>로 변환합니다.");
        MenuHelper.PrintExplanation("Fin<A>는 성공(Succ) 또는 실패(Fail)를 나타냅니다.");
        MenuHelper.PrintBlankLines();

        IO<int> RiskyDivision(int a, int b) => IO.lift(() =>
        {
            if (b == 0) throw new DivideByZeroException();
            return a / b;
        });

        MenuHelper.PrintCode(@"var safeDivision = RiskyDivision(10, 2).Try().runFin.As();
var divResult = safeDivision.Run();");

        // 성공 케이스
        var safeDivision = RiskyDivision(10, 2).Try().runFin.As();
        var divResult = safeDivision.Run();
        divResult.Match(
            Succ: v => MenuHelper.PrintResult("성공", v),
            Fail: e => MenuHelper.PrintError($"실패: {e.Message}")
        );

        // 실패 케이스
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("var failDivision = RiskyDivision(10, 0).Try().runFin.As();");
        var failDivision = RiskyDivision(10, 0).Try().runFin.As();
        var failResult = failDivision.Run();
        failResult.Match(
            Succ: v => MenuHelper.PrintResult("성공", v),
            Fail: e => MenuHelper.PrintResult("실패", e.Message)
        );

        // IfFail로 기본값 제공
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("var withDefault = RiskyDivision(10, 0).Try().runFin.As().Run().IfFail(0);");
        var withDefault = RiskyDivision(10, 0).Try().runFin.As().Run().IfFail(0);
        MenuHelper.PrintResult("기본값 사용", withDefault);

        // ============================================================
        // 5. 중첩 Bracket - 여러 리소스 순차 관리
        // ============================================================
        MenuHelper.PrintSubHeader("5. 중첩 Bracket - 여러 리소스 순차 관리");

        MenuHelper.PrintExplanation("여러 리소스를 순차적으로 획득하고 역순으로 해제합니다.");
        MenuHelper.PrintBlankLines();

        IO<DbConnection> OpenConnection() => IO.lift(() =>
        {
            var id = Interlocked.Increment(ref _connectionCounter);
            Console.WriteLine($"    [연결 열기] DB 연결 {id}");
            return new DbConnection(id);
        });

        IO<Unit> CloseConnection(DbConnection conn) => IO.lift(() =>
        {
            conn.IsConnected = false;
            Console.WriteLine($"    [연결 닫기] DB 연결 {conn.Id}");
            return unit;
        });

        IO<DbTransaction> BeginTransaction(DbConnection conn) => IO.lift(() =>
        {
            var txId = $"TX-{conn.Id}-{Guid.NewGuid().ToString()[..8]}";
            Console.WriteLine($"    [트랜잭션 시작] {txId}");
            return new DbTransaction(conn, txId);
        });

        IO<Unit> CommitTransaction(DbTransaction tx) => IO.lift(() =>
        {
            tx.IsActive = false;
            Console.WriteLine($"    [트랜잭션 커밋] {tx.TxId}");
            return unit;
        });

        IO<Unit> RollbackTransaction(DbTransaction tx) => IO.lift(() =>
        {
            tx.IsActive = false;
            Console.WriteLine($"    [트랜잭션 롤백] {tx.TxId}");
            return unit;
        });

        MenuHelper.PrintCode(@"// 중첩 Bracket: 연결 -> 트랜잭션 순서로 획득, 역순 해제
var nestedBracket = OpenConnection()
    .Bracket(
        Use: conn => BeginTransaction(conn)
            .Bracket(
                Use: tx => ExecuteQuery(tx),
                Fin: tx => CommitTransaction(tx)
            ),
        Fin: conn => CloseConnection(conn)
    );");

        IO<int> ExecuteQuery(DbTransaction tx) => IO.lift(() =>
        {
            Console.WriteLine($"    [쿼리 실행] {tx.TxId}에서 데이터 조회");
            return 42;
        });

        var nestedBracket = OpenConnection()
            .Bracket(
                Use: conn => BeginTransaction(conn)
                    .Bracket(
                        Use: tx => ExecuteQuery(tx),
                        Fin: tx => CommitTransaction(tx)
                    ),
                Fin: conn => CloseConnection(conn)
            );

        MenuHelper.PrintBlankLines();
        var queryResult = nestedBracket.Run();
        MenuHelper.PrintResult("쿼리 결과", queryResult);

        // ============================================================
        // 6. Catch와 조건부 정리 - 실패 시에만 정리
        // ============================================================
        MenuHelper.PrintSubHeader("6. Catch와 조건부 정리");

        MenuHelper.PrintExplanation("Catch를 사용하여 실패 시에만 정리 작업을 실행합니다.");
        MenuHelper.PrintExplanation("성공 시에는 리소스를 유지해야 할 때 유용합니다.");
        MenuHelper.PrintBlankLines();

        IO<DbTransaction> CreateSavepoint(DbConnection conn) => IO.lift(() =>
        {
            var txId = $"SAVEPOINT-{Guid.NewGuid().ToString()[..8]}";
            Console.WriteLine($"    [세이브포인트 생성] {txId}");
            return new DbTransaction(conn, txId);
        });

        // 성공 케이스: 롤백 실행 안 됨
        MenuHelper.PrintCode(@"// 성공 시: Catch 핸들러 실행 안 됨
var successOp = CreateSavepoint(conn)
    .Bind(sp => DoWork(sp))
    .Catch(e => { Rollback(); return IO.fail<string>(e); });");

        var conn1 = new DbConnection(1);
        var sp1 = CreateSavepoint(conn1).Run();

        var successOp = IO.lift(() =>
        {
            Console.WriteLine($"    [작업 성공] {sp1.TxId}");
            return "성공 데이터";
        }).Catch(e =>
        {
            Console.WriteLine($"    [롤백] {sp1.TxId} (실패 시에만)");
            return IO.fail<string>(e);
        });

        var successResult = successOp.Run();
        MenuHelper.PrintResult("결과", successResult);

        // 실패 케이스: 롤백 실행됨
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// 실패 시: Catch 핸들러에서 정리 작업 실행");

        var conn2 = new DbConnection(2);
        var sp2 = CreateSavepoint(conn2).Run();

        var failOp = IO.lift<string>(() =>
        {
            Console.WriteLine($"    [작업 실패] {sp2.TxId}");
            throw new Exception("작업 중 에러 발생");
        }).Catch(e =>
        {
            Console.WriteLine($"    [롤백] {sp2.TxId} (실패로 인해 롤백)");
            return IO.fail<string>(e);
        });

        try
        {
            failOp.Run();
        }
        catch (Exception ex)
        {
            MenuHelper.PrintResult("예외", ex.Message);
        }

        // ============================================================
        // 7. 복합 리소스 관리 - 헬퍼 함수
        // ============================================================
        MenuHelper.PrintSubHeader("7. 복합 리소스 관리 - 헬퍼 함수");

        MenuHelper.PrintExplanation("재사용 가능한 리소스 관리 헬퍼를 만들 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // 제네릭 리소스 관리 헬퍼
        IO<TResult> UsingResource<TResource, TResult>(
            IO<TResource> acquire,
            Func<TResource, IO<TResult>> use,
            Func<TResource, IO<Unit>> release
        ) => acquire.Bracket(Use: use, Fin: release);

        // 헬퍼 사용
        MenuHelper.PrintCode(@"var result = UsingResource(
    acquire: OpenFile(""report.txt""),
    use: file => ProcessFile(file),
    release: file => CloseFile(file)
);");

        IO<string> ProcessFile(SimulatedFile file) => IO.lift(() =>
        {
            Console.WriteLine($"    [처리] {file.Path} 파일 처리 중...");
            return $"처리된 내용: {file.Path}";
        });

        var helperResult = UsingResource(
            acquire: OpenFile("report.txt"),
            use: file => ProcessFile(file),
            release: file => CloseFile(file)
        );

        MenuHelper.PrintResult("결과", helperResult.Run());

        // ============================================================
        // 8. 실전 예제: DB 작업 시뮬레이션
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: DB 작업 시뮬레이션");

        MenuHelper.PrintExplanation("실제 DB 작업처럼 연결 -> 트랜잭션 -> 쿼리 -> 커밋/롤백");
        MenuHelper.PrintBlankLines();

        IO<string> InsertUser(DbTransaction tx, string name) => IO.lift(() =>
        {
            Console.WriteLine($"    [INSERT] 사용자 '{name}' 추가 (TX: {tx.TxId})");
            return $"user-{Guid.NewGuid().ToString()[..8]}";
        });

        IO<int> UpdateUserAge(DbTransaction tx, string userId, int age) => IO.lift(() =>
        {
            Console.WriteLine($"    [UPDATE] {userId}의 나이를 {age}로 변경 (TX: {tx.TxId})");
            return 1; // affected rows
        });

        IO<Unit> ValidateUser(string userId) => IO.lift(() =>
        {
            Console.WriteLine($"    [VALIDATE] {userId} 검증 중...");
            // 시뮬레이션: 50% 확률로 검증 실패
            // 여기서는 항상 성공하도록
            return unit;
        });

        var dbOperation = OpenConnection()
            .Bracket(
                Use: conn =>
                    from tx in BeginTransaction(conn)
                        .Bracket(
                            Use: transaction =>
                                from userId in InsertUser(transaction, "홍길동")
                                from _ in ValidateUser(userId)
                                from affected in UpdateUserAge(transaction, userId, 30)
                                select new { UserId = userId, Affected = affected },
                            Fin: transaction => CommitTransaction(transaction)
                        )
                    select tx,
                Fin: conn => CloseConnection(conn)
            );

        MenuHelper.PrintBlankLines();
        Console.WriteLine("  [DB 작업 시작]");
        var dbResult = dbOperation.Run();
        Console.WriteLine("  [DB 작업 완료]");
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintResult("생성된 사용자 ID", dbResult.UserId);
        MenuHelper.PrintResult("영향받은 행", dbResult.Affected);

        MenuHelper.PrintSuccess("리소스 관리 학습 완료!");
    }
}
