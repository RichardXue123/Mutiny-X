import os
import glob
import subprocess

def test_compile():
    cs_files = [os.path.abspath(f) for f in glob.glob('Assets/Mutiny/Scripts/**/*.cs', recursive=True) if 'Editor' not in f]
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

    item_groups = '\n'.join([f'    <Compile Include="{f}" />' for f in cs_files])
    refs = '\n'.join([f'    <Reference Include="{os.path.splitext(d)[0]}"><HintPath>{os.path.join(unity_dll_dir, d)}</HintPath></Reference>' for d in dlls])

    proj_content = f'''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
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

    temp_dir = os.path.abspath('Temp/TestCompile')
    os.makedirs(temp_dir, exist_ok=True)
    proj_path = os.path.join(temp_dir, 'TestCompile.csproj')
    with open(proj_path, 'w', encoding='utf-8') as fp:
        fp.write(proj_content)

    res = subprocess.run(['dotnet', 'build', proj_path], capture_output=True, text=True, encoding='utf-8', errors='replace')
    print('Runtime Build Exit code:', res.returncode)
    if res.returncode != 0:
        print(res.stdout)
        if res.stderr: print(res.stderr)
        return False
    print('Runtime scripts compiled cleanly.')

    # Test compile all scripts including Editor
    all_cs_files = [os.path.abspath(f) for f in glob.glob('Assets/Mutiny/Scripts/**/*.cs', recursive=True)]
    managed_dir = r'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Managed'
    editor_dlls = [
        os.path.join(managed_dir, 'UnityEngine', 'UnityEditor.CoreModule.dll')
    ]
    all_refs = refs + '\n' + '\n'.join([f'    <Reference Include="{os.path.splitext(os.path.basename(d))[0]}"><HintPath>{d}</HintPath></Reference>' for d in editor_dlls])
    all_items = '\n'.join([f'    <Compile Include="{f}" />' for f in all_cs_files])

    editor_proj_content = f'''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
    <DefineConstants>UNITY_EDITOR</DefineConstants>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
{all_refs}
  </ItemGroup>
  <ItemGroup>
{all_items}
  </ItemGroup>
</Project>'''

    editor_proj_path = os.path.join(temp_dir, 'TestCompileEditor.csproj')
    with open(editor_proj_path, 'w', encoding='utf-8') as fp:
        fp.write(editor_proj_content)

    res2 = subprocess.run(['dotnet', 'build', editor_proj_path], capture_output=True, text=True, encoding='utf-8', errors='replace')
    print('Editor Build Exit code:', res2.returncode)
    if res2.returncode != 0:
        print(res2.stdout)
        if res2.stderr: print(res2.stderr)
        return False
    print('All Editor & Runtime scripts compiled cleanly.')
    return True

if __name__ == '__main__':
    test_compile()
