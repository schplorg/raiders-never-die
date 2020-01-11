#!/bin/bash
/c/Windows/Microsoft.NET/Framework64/v4.0.30319/MSBuild.exe RaidersNeverDie.sln
rm -rf /c/Program\ Files\ \(x86\)/Steam/steamapps/common/RimWorld/Mods/RaidersNeverDie
mkdir /c/Program\ Files\ \(x86\)/Steam/steamapps/common/RimWorld/Mods/RaidersNeverDie
cp -rf ./About /c/Program\ Files\ \(x86\)/Steam/steamapps/common/RimWorld/Mods/RaidersNeverDie/
cp -rf ./Assemblies /c/Program\ Files\ \(x86\)/Steam/steamapps/common/RimWorld/Mods/RaidersNeverDie/