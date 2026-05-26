{
  pkgs ? import <nixpkgs> { },
}:
pkgs.mkShell {
  packages = with pkgs; [
    dotnet-sdk_9
  ];

  DOTNET_ROOT = "${pkgs.dotnet-sdk_9}/share/dotnet";
}
