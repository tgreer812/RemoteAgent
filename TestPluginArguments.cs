using System;
using AgentCommon.AgentPluginCommon;
using Newtonsoft.Json.Linq;

class TestPluginArguments
{
    static void Main()
    {
        Console.WriteLine("Testing PluginArguments refactoring...");

        // Test 1: Create empty PluginArguments
        var args1 = new PluginArguments();
        args1.AddArgument("test", "value");
        args1.AddArgument("number", 42);
        Console.WriteLine($"Test 1 - Added arguments: {args1.ToJson()}");

        // Test 2: Create from Dictionary
        var dict = new System.Collections.Generic.Dictionary<string, object>
        {
            {"path", "C:\\test"},
            {"count", 100},
            {"enabled", true}
        };
        var args2 = new PluginArguments(dict);
        Console.WriteLine($"Test 2 - From dictionary: {args2.ToJson()}");

        // Test 3: Create from JSON
        var json = "{\"pluginName\":\"DirectoryList\",\"recursive\":true,\"maxDepth\":5}";
        var args3 = PluginArguments.FromJson(json);
        Console.WriteLine($"Test 3 - From JSON: {args3.ToJson()}");

        // Test 4: Create from JObject (backward compatibility)
        var jObject = JObject.Parse(json);
        var args4 = PluginArguments.FromJObject(jObject);
        Console.WriteLine($"Test 4 - From JObject: {args4.ToJson()}");

        // Test 5: Test type conversion
        var testValue = args3.GetArgument<string>("pluginName");
        var testBool = args3.GetArgument<bool>("recursive");
        var testInt = args3.GetArgument<int>("maxDepth");
        Console.WriteLine($"Test 5 - Type conversion: {testValue}, {testBool}, {testInt}");

        // Test 6: Test backward compatibility (ToJObject)
        var backwardJObject = args3.ToJObject();
        Console.WriteLine($"Test 6 - ToJObject: {backwardJObject}");

        Console.WriteLine("All tests completed successfully!");
    }
}
