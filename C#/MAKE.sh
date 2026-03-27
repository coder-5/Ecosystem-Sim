#!/bin/bash

APP_NAME="EcosystemSim"
OUT_DIR="build"

echo "Cleaning old builds"
rm -rf $OUT_DIR

echo "Building for Windows..."
dotnet publish -c Release -r win-x64 --self-contained true \
/p:PublishSingleFile=true -o $OUT_DIR/win

echo "Building for Linux..."
dotnet publish -c Release -r linux-x64 --self-contained true \
/p:PublishSingleFile=true -o $OUT_DIR/linux

echo "Building for macOS (Intel)..."
dotnet publish -c Release -r osx-x64 --self-contained true \
/p:PublishSingleFile=true -o $OUT_DIR/osx-x64

echo "Building for macOS (Apple Silicon)..."
dotnet publish -c Release -r osx-arm64 --self-contained true \
/p:PublishSingleFile=true -o $OUT_DIR/osx-arm64

echo "Adding chmod permissions"
chmod +x $OUT_DIR/osx-arm64/$APP_NAME
chmod +x $OUT_DIR/osx-x64/$APP_NAME
chmod +x $OUT_DIR/linux/$APP_NAME

echo "Zipping all builds"
zip -r $OUT_DIR/osx-arm64.zip $OUT_DIR/osx-arm64
zip -r $OUT_DIR/osx-x64.zip $OUT_DIR/osx-x64
zip -r $OUT_DIR/linux-x64.zip $OUT_DIR/linux
zip -r $OUT_DIR/win.zip $OUT_DIR/win

echo "Done. Outputs are in /build folder."

# command to run ./MAKE.sh 