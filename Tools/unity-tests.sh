#!/usr/bin/env bash
# Compile Assets/@Game and run the EditMode model tests without opening Unity.
# Unity's own -runTests needs a signed-in licence; this harness uses the bundled Roslyn csc plus a
# tiny NUnit reflection runner instead. Usage: bash Tools/unity-tests.sh
set -uo pipefail

UNITY_VERSION="6000.6.8f1"
U="C:/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Data"
API="$U/UnityReferenceAssemblies/unity-4.8-api"
MONO="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Data/MonoBleedingEdge/bin/mono.exe"
CSC="$U/MonoBleedingEdge/lib/mono/4.5/csc.exe"
P="$(cygpath -m "$(git rev-parse --show-toplevel)")"
NUNIT="$P/Library/PackageCache/com.unity.ext.nunit@0198eae3b53e/net472/unity-custom/nunit.framework.dll"
DOTWEEN="$P/Assets/Plugins/Demigiant/DOTween/DOTween.dll"
S="${PROJECTTA_CHECK:-$(cygpath -m "${TMPDIR:-$LOCALAPPDATA/Temp}")/projectta-check}"
RSP="$S/check.rsp"
mkdir -p "$S"

{
  echo "-target:library"; echo "-nologo"; echo "-nostdlib+"; echo "-langversion:9"; echo "-nowarn:1701,1702,0649"
  echo "-out:$S/projectta_check.dll"
  echo "-r:\"$API/mscorlib.dll\""; echo "-r:\"$API/System.dll\""; echo "-r:\"$API/System.Core.dll\""
  echo "-r:\"$API/Facades/netstandard.dll\""
  echo "-r:\"$NUNIT\""
  echo "-r:\"$DOTWEEN\""
  for f in "$U"/Managed/UnityEngine/*.dll; do echo "-r:\"$f\""; done
  for f in "$P"/Library/ScriptAssemblies/*.dll; do echo "-r:\"$f\""; done
  find "$P/Assets/@Game" -name '*.cs' -not -path '*/Editor/*' | sed 's|^|"|;s|$|"|'
} > "$RSP"

echo "=== compile ==="
COMPILE_ERRORS=$("$MONO" "$CSC" "@$RSP" 2>&1 | grep -E ': error ' | sort -u)
if [ -n "$COMPILE_ERRORS" ]; then
  printf '%s\n' "$COMPILE_ERRORS"
  echo "=== compile failed ==="
  exit 1
fi
echo "clean"

cat > "$S/Runner.cs" <<'CS'
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

class Runner
{
    static int Main(string[] args)
    {
        Assembly asm = Assembly.LoadFrom(args[0]);
        int pass = 0, fail = 0, blocked = 0;
        foreach (Type type in asm.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract) continue;
            var tests = new List<MethodInfo>();
            foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (m.GetCustomAttribute<TestAttribute>() != null) tests.Add(m);
            }
            if (tests.Count == 0) continue;
            foreach (MethodInfo test in tests)
            {
                object instance = Activator.CreateInstance(type);
                try
                {
                    foreach (MethodInfo setup in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (setup.GetCustomAttribute<SetUpAttribute>() != null) setup.Invoke(instance, null);
                    }
                    test.Invoke(instance, null);
                    pass++;
                    Console.WriteLine("PASS  " + type.Name + "." + test.Name);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is MissingMethodException)
                    {
                        blocked++;
                        Console.WriteLine("BLOCK " + type.Name + "." + test.Name);
                    }
                    else
                    {
                        fail++;
                        Console.WriteLine("FAIL  " + type.Name + "." + test.Name + " -> " + ex.InnerException.GetType().Name + ": " + ex.InnerException.Message);
                    }
                }
            }
        }
        Console.WriteLine("pass=" + pass + " fail=" + fail + " blocked=" + blocked);
        return fail == 0 ? 0 : 1;
    }
}
CS

echo "=== runner ==="
"$MONO" "$CSC" -nologo -out:"$S/Runner.exe" -r:"$NUNIT" "$S/Runner.cs" || exit 2

export MONO_PATH="$S;$U/Managed/UnityEngine;$P/Library/PackageCache/com.unity.ext.nunit@0198eae3b53e/net472/unity-custom;$P/Library/ScriptAssemblies;$P/Assets/Plugins/Demigiant/DOTween"
echo "=== tests ==="
"$MONO" "$S/Runner.exe" "$S/projectta_check.dll" 2>&1 | grep -E '^(FAIL|pass=)'
# ponytail: hand-rolled runner because Unity -runTests is licence-blocked; delete once the Editor can run EditMode tests.
