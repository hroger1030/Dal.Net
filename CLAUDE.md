# CLAUDE.md

## Project overview

This repository contains a .NET solution for making DB calls and presenting simple ORM objects.

## Stack

- C#
- Microsoft .NET 8
- Newtonsoft JSON parser
- NUnit tests 
- To be built for a Windows platform

## Development guidance

- Keep changes compatible with .NET 8.
- There should be only one namespace per project, and it should match the project name.
- No private functions allowed. methods should be public to allow unit tests to be written.
- Functions should use dependency injection to allow for easy testing.
- If you add new features, update this file to reflect the new workflow.

## Non-negotiables

- Don't upgrade any nuget package version without asking first. You can point out out of date packages to the user.
- Don't do write anything to git. You can read all you want, but no writes or commits.
- when refactoring existing code, do not remove comments. They can be updated if needed, but not removed.