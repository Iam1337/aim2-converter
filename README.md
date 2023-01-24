# AIM2 Converter - Convert AIM2 resources to Unity Engine

Created by [iam1337](https://github.com/iam1337). Based on [Polygon-4 tools](https://github.com/aimrebirth/tools).

![](https://img.shields.io/badge/unity-2020.3%20or%20later-green.svg)
[![⚙ Build and Release](https://github.com/Iam1337/aim2-converter/actions/workflows/ci.yml/badge.svg)](https://github.com/Iam1337/aim2-converter/actions/workflows/ci.yml)
[![openupm](https://img.shields.io/npm/v/com.iam1337.aim2-converter?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.iam1337.aim2-converter/)
[![](https://img.shields.io/github/license/iam1337/aim2-converter.svg)](https://github.com/Iam1337/aim2-converter/blob/master/LICENSE)
[![semantic-release: angular](https://img.shields.io/badge/semantic--release-angular-e10079?logo=semantic-release)](https://github.com/semantic-release/semantic-release)

### Table of Contents
- [Introduction](#introduction)
- [Installation](#installation)
- [Usage](#usage)
- [Author Contacts](#author-contacts)

## Introduction

AIM2 Converter is a tool for converting resources from the AIM2 game to Unity Engine, with the API to extend the import functions.

![AIM2 Converter](https://i.imgur.com/y7qOLWL.png)

### Features:

- **Import models**<br>
Import models from the game by converting them directly into prefabs.
- **Import textures**<br>
With the extension API, you can automate the import of special materials. (By default, all materials are Standard (Specular))

## Installation
**Old school**

Just copy the [Assets/aim2-converter](Assets/aim2-converter) folder into your Assets directory within your Unity project.

**OpenUPM**

Via [openupm-cli](https://github.com/openupm/openupm-cli):<br>
```
openupm add com.iam1337.aim2-converter
```

Or if you don't have it, add the scoped registry to manifest.json with the desired dependency semantic version:
```
"scopedRegistries": [
	{
		"name": "package.openupm.com",
		"url": "https://package.openupm.com",
		"scopes": [
			"com.iam1337.aim2-converter",
		]
	}
],
"dependencies": {
	"com.iam1337.aim2-converter": "1.0.0"
}
```

**Package Manager**

Project supports Unity Package Manager. To install the project as a Git package do the following:

1. In Unity, open **Window > Package Manager**.
2. Press the **+** button, choose **"Add package from git URL..."**
3. Enter "https://github.com/iam1337/aim-converter.git#upm" and press Add.
4. Install package from Package Manager.

## Usage

WIP

## Author Contacts
\> [telegram.me/iam1337](http://telegram.me/iam1337) <br>
\> [ext@iron-wall.org](mailto:ext@iron-wall.org)

## License
This project is under the MIT License.
