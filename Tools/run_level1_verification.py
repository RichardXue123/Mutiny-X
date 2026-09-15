import os
import subprocess
import glob

def run_verification():
    unity_dll_dir = r'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Managed\UnityEngine'
    dlls = [
        'UnityEngine.dll',
        'UnityEngine.CoreModule.dll',
        'UnityEngine.Physics2DModule.dll',
        'UnityEngine.AudioModule.dll',
        'UnityEngine.TextRenderingModule.dll',
        'UnityEngine.UIElementsModule.dll',
        'UnityEngine.IMGUIModule.dll',
        'UnityEngine.InputLegacyModule.dll',
        'UnityEngine.InputModule.dll'
    ]

    cs_files = [os.path.abspath(f) for f in glob.glob('Assets/Mutiny/Scripts/**/*.cs', recursive=True) if 'Editor' not in f]

    main_cs = os.path.abspath('Temp/TestCompile/VerificationRunner.cs')
    os.makedirs(os.path.dirname(main_cs), exist_ok=True)

    with open(main_cs, 'w', encoding='utf-8') as fp:
        fp.write('''using System;
using Mutiny.Verification;

class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("=== RUNNING LEVEL 1 VERIFICATION ===");
        var result = MutinyLevel1VerificationTest.RunAllTests("Assets/Mutiny/Data/Levels/level_01.xml");
        
        foreach (var log in result.Logs)
        {
            Console.WriteLine(log);
        }

        if (result.Failures.Count > 0)
        {
            Console.WriteLine("FAILURES:");
            foreach (var fail in result.Failures)
            {
                Console.WriteLine(fail);
            }
            return 1;
        }

        Console.WriteLine($"RESULT: ALL {result.TotalAssertions} ASSERTIONS PASSED!");
        return 0;
    }
}
''')

    cs_files.append(main_cs)
    item_groups = '\n'.join([f'    <Compile Include="{f}" />' for f in cs_files])
    refs = '\n'.join([f'    <Reference Include="{os.path.splitext(d)[0]}"><HintPath>{os.path.join(unity_dll_dir, d)}</HintPath></Reference>' for d in dlls])

    proj_content = f'''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
{refs}
  </ItemGroup>
  <ItemGroup>
{item_groups}
  </ItemGroup>
</Project>'''

    proj_path = os.path.abspath('Temp/TestCompile/VerificationRunner.csproj')
    with open(proj_path, 'w', encoding='utf-8') as fp:
        fp.write(proj_content)

    build_res = subprocess.run(['dotnet', 'run', '--project', proj_path], capture_output=True, text=True, encoding='utf-8', errors='replace')
    print(build_res.stdout)
    if build_res.stderr:
        print("STDERR:", build_res.stderr)
    return build_res.returncode == 0

if __name__ == '__main__':
    run_verification()

