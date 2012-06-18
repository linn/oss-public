
using System;
using Linn;
using Linn.TestFramework;


public class SuiteOptionParser : Suite
{
    public SuiteOptionParser() : base("Tests for option parser") {
    }

    public override void Test() {
        // Option creation tests

        // no option names
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            null, null, "defaultstring", "test string help", "STRING");

        // bad short option names
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            "", null, "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            "s", null, "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            "-", null, "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            "--", null, "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            "-sd", null, "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            "--sd", null, "defaultstring", "test string help", "STRING");

        // bad long option names
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            null, "", "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            null, "s", "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            null, "-", "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            null, "-s", "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            null, "--", "defaultstring", "test string help", "STRING");
        TEST_THROWS_NEW(typeof(AssertionError), typeof(OptionParser.OptionString),
            null, "-sd", "defaultstring", "test string help", "STRING");

        // Some tests for different option types
        OptionStringTests();
        OptionIntTests();
        OptionBoolTests();

        // Parsing tests
        OptionParser optParser = new OptionParser(new string[] {"-a", "aval", "--bc", "bcval"});
        OptionParser.OptionString optString1 = new OptionParser.OptionString("-a", null, "defaultstring", "string help", "STRING");
        OptionParser.OptionString optString2 = new OptionParser.OptionString(null, "--bc", "defaultstring", "string help", "STRING");
        OptionParser.OptionString optString3 = new OptionParser.OptionString("-a", "--bc", "defaultstring", "string help", "STRING");
        OptionParser.OptionInt optInt4 = new OptionParser.OptionInt("-a", null, 123, "int help", "INT");
        OptionParser.OptionBool optBool5 = new OptionParser.OptionBool(null, "--bc", "bool help");

        // test adding already existing options
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        TEST_THROWS(typeof(AssertionError), optParser, "AddOption", optString1);
        TEST_THROWS(typeof(AssertionError), optParser, "AddOption", optString2);
        TEST_THROWS(typeof(AssertionError), optParser, "AddOption", optString3);
        TEST_THROWS(typeof(AssertionError), optParser, "AddOption", optInt4);
        TEST_THROWS(typeof(AssertionError), optParser, "AddOption", optBool5);

        optParser.Parse();
        TEST(optParser.PosArgs.Count == 0);
        TEST(optString1.Value == "aval");
        TEST(optString2.Value == "bcval");
        TEST(optString3.Value == "defaultstring");
        TEST(optInt4.Value == 123);
        TEST(optBool5.Value == false);

        // test undefined options
        optParser = new OptionParser(new string[] {"-d"});
        TEST_THROWS(typeof(OptionParser.OptionParserError), optParser, "Parse");
        TEST(optParser.PosArgs.Count == 0);

        optParser = new OptionParser(new string[] {"--de"});
        TEST_THROWS(typeof(OptionParser.OptionParserError), optParser, "Parse");
        TEST(optParser.PosArgs.Count == 0);
    }

    private void OptionStringTests() {
        // string option tests
        OptionParser optParser;
        OptionParser.OptionString optString1 = new OptionParser.OptionString("-a", "--stringa", "defaultstring1", "string help", "STRING");
        OptionParser.OptionString optString2 = new OptionParser.OptionString("-b", "--stringb", "defaultstring2", "string help", "STRING");
        TEST(optString1.ShortName == "-a");
        TEST(optString1.LongName == "--stringa");
        TEST(optString1.Value == "defaultstring1");
        TEST(optString2.ShortName == "-b");
        TEST(optString2.LongName == "--stringb");
        TEST(optString2.Value == "defaultstring2");

        // no args
        optParser = new OptionParser(new string[] {});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 0);
        TEST(optString1.Value == "defaultstring1");
        TEST(optString2.Value == "defaultstring2");

        // positional args only
        optParser = new OptionParser(new string[] {"arg1", "arg2"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optString1.Value == "defaultstring1");
        TEST(optString2.Value == "defaultstring2");

        // 1 good arg
        optParser = new OptionParser(new string[] {"-a", "stringa", "arg1", "arg2"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optString1.Value == "stringa");
        TEST(optString2.Value == "defaultstring2");

        optParser = new OptionParser(new string[] {"--stringa", "stringa", "arg1", "arg2"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optString1.Value == "stringa");
        TEST(optString2.Value == "defaultstring2");

        optParser = new OptionParser(new string[] {"arg1", "-a", "stringa", "arg2"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optString1.Value == "stringa");
        TEST(optString2.Value == "defaultstring2");

        optParser = new OptionParser(new string[] {"arg1", "arg2", "-a", "stringa"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optString1.Value == "stringa");
        TEST(optString2.Value == "defaultstring2");

        // 2 good args
        optParser = new OptionParser(new string[] {"-b", "stringb", "-a", "stringa", "arg1", "arg2"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optString1.Value == "stringa");
        TEST(optString2.Value == "stringb");

        optParser = new OptionParser(new string[] {"-b", "stringb", "arg1", "arg2", "-a", "stringa"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optString1.Value == "stringa");
        TEST(optString2.Value == "stringb");

        // missing option value
        optParser = new OptionParser(new string[] {"-b", "-a", "stringa", "arg1", "arg2"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        TEST_THROWS(typeof(OptionParser.OptionParserError), optParser, "Parse");
        TEST(optParser.PosArgs.Count == 0);
        TEST(optString1.Value == "defaultstring1");
        TEST(optString2.Value == "defaultstring2");

        optParser = new OptionParser(new string[] {"-b", "stringb", "arg1", "arg2", "-a"});
        optParser.AddOption(optString1);
        optParser.AddOption(optString2);
        TEST_THROWS(typeof(OptionParser.OptionParserError), optParser, "Parse");
        TEST(optParser.PosArgs.Count == 0);
        TEST(optString1.Value == "defaultstring1");
        TEST(optString2.Value == "defaultstring2");

        // help tests
        OptionParser.OptionHelp help;
        help = new OptionParser.OptionHelp();
        optString1 = new OptionParser.OptionString("-a", null, "defaultstring1", "string help", "STRING");
        optString1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a STRING             string help\n");

        help = new OptionParser.OptionHelp();
        optString1 = new OptionParser.OptionString(null, "--stringa", "defaultstring1", "string help", "STRING");
        optString1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  --stringa=STRING      string help\n");

        help = new OptionParser.OptionHelp();
        optString1 = new OptionParser.OptionString("-a", "--stra", "defaultstring1", "string help", "STR");
        optString1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a STR, --stra=STR    string help\n");

        help = new OptionParser.OptionHelp();
        optString1 = new OptionParser.OptionString("-a", "--stringa", "defaultstring1", "string help", "STRING");
        optString1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a STRING, --stringa=STRING\n                        string help\n");
    }

    private void OptionIntTests() {
        // int option tests
        OptionParser optParser;
        OptionParser.OptionInt optInt1 = new OptionParser.OptionInt("-a", "--inta", 123, "int help", "INT");
        OptionParser.OptionInt optInt2 = new OptionParser.OptionInt("-b", "--intb", 456, "int help", "INT");
        TEST(optInt1.ShortName == "-a");
        TEST(optInt1.LongName == "--inta");
        TEST(optInt1.Value == 123);
        TEST(optInt2.ShortName == "-b");
        TEST(optInt2.LongName == "--intb");
        TEST(optInt2.Value == 456);

        // no args
        optParser = new OptionParser(new string[] {});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 0);
        TEST(optInt1.Value == 123);
        TEST(optInt2.Value == 456);

        // positional args only
        optParser = new OptionParser(new string[] {"arg1", "arg2"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optInt1.Value == 123);
        TEST(optInt2.Value == 456);

        // 1 good arg
        optParser = new OptionParser(new string[] {"-a", "789", "arg1", "arg2"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optInt1.Value == 789);
        TEST(optInt2.Value == 456);

        optParser = new OptionParser(new string[] {"--inta", "789", "arg1", "arg2"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optInt1.Value == 789);
        TEST(optInt2.Value == 456);

        optParser = new OptionParser(new string[] {"arg1", "-a", "789", "arg2"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optInt1.Value == 789);
        TEST(optInt2.Value == 456);

        optParser = new OptionParser(new string[] {"arg1", "arg2", "-a", "789"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optInt1.Value == 789);
        TEST(optInt2.Value == 456);

        // 2 good args
        optParser = new OptionParser(new string[] {"-b", "987", "-a", "789", "arg1", "arg2"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optInt1.Value == 789);
        TEST(optInt2.Value == 987);

        optParser = new OptionParser(new string[] {"-b", "987", "arg1", "arg2", "-a", "789"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optInt1.Value == 789);
        TEST(optInt2.Value == 987);

        // missing option value
        optParser = new OptionParser(new string[] {"-b", "-a", "789", "arg1", "arg2"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        TEST_THROWS(typeof(OptionParser.OptionParserError), optParser, "Parse");
        TEST(optParser.PosArgs.Count == 0);
        TEST(optInt1.Value == 123);
        TEST(optInt2.Value == 456);

        optParser = new OptionParser(new string[] {"-b", "987", "arg1", "arg2", "-a"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        TEST_THROWS(typeof(OptionParser.OptionParserError), optParser, "Parse");
        TEST(optParser.PosArgs.Count == 0);
        TEST(optInt1.Value == 123);
        TEST(optInt2.Value == 456);

        // bad option value
        optParser = new OptionParser(new string[] {"-b", "98w7", "-a", "789", "arg1", "arg2"});
        optParser.AddOption(optInt1);
        optParser.AddOption(optInt2);
        TEST_THROWS(typeof(OptionParser.OptionParserError), optParser, "Parse");
        TEST(optParser.PosArgs.Count == 0);
        TEST(optInt1.Value == 123);
        TEST(optInt2.Value == 456);

        // help tests
        OptionParser.OptionHelp help;
        help = new OptionParser.OptionHelp();
        optInt1 = new OptionParser.OptionInt("-a", null, 123, "int help", "INT");
        optInt1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a INT                int help\n");

        help = new OptionParser.OptionHelp();
        optInt1 = new OptionParser.OptionInt(null, "--inta", 123, "int help", "INT");
        optInt1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  --inta=INT            int help\n");

        help = new OptionParser.OptionHelp();
        optInt1 = new OptionParser.OptionInt("-a", "--inta", 123, "int help", "INT");
        optInt1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a INT, --inta=INT    int help\n");

        help = new OptionParser.OptionHelp();
        optInt1 = new OptionParser.OptionInt("-a", "--inta", 123, "int help", "INTEGER");
        optInt1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a INTEGER, --inta=INTEGER\n                        int help\n");
    }

    private void OptionBoolTests() {
        // bool option tests
        OptionParser optParser;
        OptionParser.OptionBool optBool1 = new OptionParser.OptionBool("-a", "--boola", "bool help");
        OptionParser.OptionBool optBool2 = new OptionParser.OptionBool("-b", "--boolb", "bool help");
        TEST(optBool1.ShortName == "-a");
        TEST(optBool1.LongName == "--boola");
        TEST(optBool1.Value == false);
        TEST(optBool2.ShortName == "-b");
        TEST(optBool2.LongName == "--boolb");
        TEST(optBool2.Value == false);

        // no args
        optParser = new OptionParser(new string[] {});
        optParser.AddOption(optBool1);
        optParser.AddOption(optBool2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 0);
        TEST(optBool1.Value == false);
        TEST(optBool2.Value == false);

        // positional args only
        optParser = new OptionParser(new string[] {"arg1", "arg2"});
        optParser.AddOption(optBool1);
        optParser.AddOption(optBool2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optBool1.Value == false);
        TEST(optBool2.Value == false);

        // 1 good arg
        optParser = new OptionParser(new string[] {"-a", "arg1", "arg2"});
        optParser.AddOption(optBool1);
        optParser.AddOption(optBool2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optBool1.Value == true);
        TEST(optBool2.Value == false);

        optParser = new OptionParser(new string[] {"--boola", "arg1", "arg2"});
        optParser.AddOption(optBool1);
        optParser.AddOption(optBool2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optBool1.Value == true);
        TEST(optBool2.Value == false);

        optParser = new OptionParser(new string[] {"arg1", "-a", "arg2"});
        optParser.AddOption(optBool1);
        optParser.AddOption(optBool2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optBool1.Value == true);
        TEST(optBool2.Value == false);

        optParser = new OptionParser(new string[] {"arg1", "arg2", "-a"});
        optParser.AddOption(optBool1);
        optParser.AddOption(optBool2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optBool1.Value == true);
        TEST(optBool2.Value == false);

        // 2 good args
        optParser = new OptionParser(new string[] {"-b", "-a", "arg1", "arg2"});
        optParser.AddOption(optBool1);
        optParser.AddOption(optBool2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optBool1.Value == true);
        TEST(optBool2.Value == true);

        optParser = new OptionParser(new string[] {"-b", "arg1", "arg2", "-a"});
        optParser.AddOption(optBool1);
        optParser.AddOption(optBool2);
        optParser.Parse();
        TEST(optParser.PosArgs.Count == 2);
        TEST(optParser.PosArgs[0] == "arg1");
        TEST(optParser.PosArgs[1] == "arg2");
        TEST(optBool1.Value == true);
        TEST(optBool2.Value == true);

        // help tests
        OptionParser.OptionHelp help;
        help = new OptionParser.OptionHelp();
        optBool1 = new OptionParser.OptionBool("-a", null, "bool help");
        optBool1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a                    bool help\n");

        help = new OptionParser.OptionHelp();
        optBool1 = new OptionParser.OptionBool(null, "--boola", "bool help");
        optBool1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  --boola               bool help\n");

        help = new OptionParser.OptionHelp();
        optBool1 = new OptionParser.OptionBool("-a", "--boola", "bool help");
        optBool1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a, --boola           bool help\n");

        help = new OptionParser.OptionHelp();
        optBool1 = new OptionParser.OptionBool("-a", "--boolaaaaaaaaaa", "bool help");
        optBool1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a, --boolaaaaaaaaaa  bool help\n");

        help = new OptionParser.OptionHelp();
        optBool1 = new OptionParser.OptionBool("-a", "--boolaaaaaaaaaaa", "bool help");
        optBool1.AppendHelp(help);
        TEST(help.ToString() == "options:\n  -a, --boolaaaaaaaaaaa\n                        bool help\n");
    }
}

class TestOptionParser
{
    static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();

        Runner runner = new Runner("Option Parser module tests");
        runner.Add( new SuiteOptionParser() );
        runner.Run();

        helper.Dispose();
    }
}


