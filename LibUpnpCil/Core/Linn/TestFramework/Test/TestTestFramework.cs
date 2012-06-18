using Linn.TestFramework;
using System;

namespace Linn {

internal class TestException : System.Exception
{
    public TestException() {
    }
}

internal class SuiteTest1 : Suite
{
    internal SuiteTest1(string aDescription) : base(aDescription) {
    }

    public override void Test() {
        TEST(1==1);
        TEST(1==1);
        TEST("hello"=="hello");
        TEST("hello"=="hello");
        TEST("hello"=="hello");
        TEST("hello"=="hello");
        TEST("hello"=="hello");
        TEST(1==2);    // FAIL
        TEST(1==2);    // FAIL
        TEST(1==1);
        TEST(1==1);
        TEST(1==1);
        TEST(1==2);    // FAIL
        TEST(1==1);
        TEST(1==1);
        TEST(1==1);
        TEST(1==1);
//        TEST_THROWS(TestException, func, 12, aArg3=45, aArg2=67);
        throw new TestException();
//        TEST(1==1);
//        TEST(1==1);
    }
}

internal class SuiteTest2 : Suite
{
    internal SuiteTest2(string aDescription) : base(aDescription) {
    }

    public override void Test() {
        TEST(1==1);
        TEST(1==1);
        TEST("hello"=="hello");
        TEST("hello"=="hello");
        TEST("hello"=="hello");
        TEST("hello"=="hello");
        TEST("hello"=="hello");
        TEST(1==2);    // FAIL
        TEST(1==2);    // FAIL
        TEST(1==1);
        TEST(1==1);
        TEST(1==1);
        TEST(1==2);    // FAIL
        TEST(1==1);
        TEST(1==1);
        TEST(1==1);
        TEST(1==1);
    }
}


class TestTestFramework
{
    static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();

        Runner runner = new Runner("TestFramework runner tests");
        runner.Add( new SuiteTest1("Test1 suite") );
        runner.Add( new SuiteTest2("Test2 suite") );
        runner.Run();

        helper.Dispose();
    }
}


} // namespace Linn
