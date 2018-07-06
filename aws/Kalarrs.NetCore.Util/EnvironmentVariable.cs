using System;
using System.Collections;
using System.Collections.Generic;

namespace Kalarrs.NetCore.Util
{
    public static class EnvironmentVariable
    {
        public static void PrepareEnvironmentVariables(IDictionary defatultEnvironmentVariables, Dictionary<string, string> serverlessEnvironmentVariables, Dictionary<string, string> functionEnvironmentVariables)
        {
            // Remove all ENV variables.
            foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
                Environment.SetEnvironmentVariable(environmentVariable.Key.ToString(), string.Empty, EnvironmentVariableTarget.Process);
            // Restore default ENV variables.
            if (defatultEnvironmentVariables != null)
                foreach (DictionaryEntry defaultEnvironmentVariable in defatultEnvironmentVariables)
                    Environment.SetEnvironmentVariable(defaultEnvironmentVariable.Key.ToString(), defaultEnvironmentVariable.Value.ToString(), EnvironmentVariableTarget.Process);
            // Set Serverless Provider Level ENV variables.
            if (serverlessEnvironmentVariables != null)
                foreach (var serverlessKeyValuePair in serverlessEnvironmentVariables)
                    Environment.SetEnvironmentVariable(serverlessKeyValuePair.Key, serverlessKeyValuePair.Value, EnvironmentVariableTarget.Process);
            // Set Serverless Function Level ENV variables.
            if (functionEnvironmentVariables != null)
                foreach (var functionKeyValuePair in functionEnvironmentVariables)
                    Environment.SetEnvironmentVariable(functionKeyValuePair.Key, functionKeyValuePair.Value, EnvironmentVariableTarget.Process);
        }
    }
}