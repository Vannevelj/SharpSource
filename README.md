[![Nuget Downloads](https://img.shields.io/nuget/dt/SharpSource)](https://www.nuget.org/packages/SharpSource/) [![Visual Studio Marketplace Downloads](https://img.shields.io/visual-studio-marketplace/d/JeroenVannevel.sharpsource)](https://marketplace.visualstudio.com/items?itemName=JeroenVannevel.sharpsource)

![Demonstration of an analyzer which removes unnecessary IEnumerable materializations](https://user-images.githubusercontent.com/2777107/190013653-edfcb61b-06a1-46d4-8b99-a71173beebb2.gif)

## Quickstart

Install it through the command line:

```powershell
Install-Package SharpSource
```

or add a reference yourself:

```xml
<ItemGroup>
    <PackageReference Include="SharpSource" Version="1.11.1" PrivateAssets="All" />
</ItemGroup>
```

If you would like to install it as an extension instead, download it [from the marketplace](https://marketplace.visualstudio.com/items?itemName=JeroenVannevel.sharpsource).

---

This repo houses a collection of analyzers that aim to make some language features and framework types easier to work with. It does this by highlighting when you might be using something incorrectly in a way that would result in suboptimal performance, runtime exceptions or general unintended behaviour. 

In other words, **this repo only contains analyzers for patterns that have a concrete potential to turn into a defect ticket**. It is not intended to help with general housekeeping tasks like formatting your code or providing productivity helpers. 

Interested in contributing? Take a look at [the guidelines](./CONTRIBUTING.md)!

---

Detailed explanations of each analyzer can be found in the documentation: https://github.com/Vannevelj/SharpSource/tree/master/docs
 

| Code   | Name |
|---|---|
| SS001  | AsyncMethodWithVoidReturnType  |
| SS002  | DateTimeNow  |
| SS003  | DivideIntegerByInteger  |
| SS004  | ElementaryMethodsOfTypeInCollectionNotOverridden | 
| SS005  | EqualsAndGetHashcodeNotImplementedTogether  | 
| SS006  | ThrowNull  |
| SS007  | FlagsEnumValuesAreNotPowersOfTwo  | 
| SS008  | GetHashCodeRefersToMutableMember  | 
| SS009  | LoopedRandomInstantiation  | 
| SS010  | NewGuid  | 
| SS011  | OnPropertyChangedWithoutNameofOperator  | 
| SS012  | RecursiveOperatorOverload  | 
| SS013  | RethrowExceptionWithoutLosingStacktrace | 
| SS014  | StringDotFormatWithDifferentAmountOfArguments  | 
| SS015  | StringPlaceholdersInWrongOrder  | 
| SS017  | StructWithoutElementaryMethodsOverridden  | 
| SS018  | SwitchDoesNotHandleAllEnumOptions  |
| SS019  | SwitchIsMissingDefaultLabel  |
| SS020  | TestMethodWithoutPublicModifier  | 
| SS021  | TestMethodWithoutTestAttribute  | 
| SS022  | ExceptionThrownFromImplicitOperator  | 
| SS023  | ExceptionThrownFromPropertyGetter  |
| SS024  | ExceptionThrownFromStaticConstructor  | 
| SS025  | ExceptionThrownFromFinallyBlock  | 
| SS026  | ExceptionThrownFromEqualityOperator  |
| SS027  | ExceptionThrownFromDispose   | 
| SS028  | ExceptionThrownFromFinalizer  |
| SS029  | ExceptionThrownFromGetHashCode |
| SS030  | ExceptionThrownFromEquals  | 
| SS032  | ThreadSleepInAsyncMethod  | 
| SS033  | AsyncOverloadsAvailable  | 
| SS034  | AccessingTaskResultWithoutAwait  |
| SS035  | SynchronousTaskWait  | 
| SS036  | ExplicitEnumValues  | 
| SS037  | HttpClientInstantiatedDirectly  | 
| SS038  | HttpContextStoredInField  | 
| SS039  | EnumWithoutDefaultValue  | 
| SS040  | UnusedResultOnImmutableObject  | 
| SS041  | UnnecessaryEnumerableMaterialization  | 
| SS042  | InstanceFieldWithThreadStatic  | 
| SS043  | MultipleFromBodyParameters  | 
| SS044  | AttributeMustSpecifyAttributeUsage  | 
| SS045  | StaticInitializerAccessedBeforeInitialization  | 
| SS046  | UnboundedStackalloc  | 
| SS047  | LinqTraversalBeforeFilter  | 
| SS048  | LockingOnDiscouragedObject  | 
| SS049  | ComparingStringsWithoutStringComparison  | 

## Configuration
Is a particular rule not to your liking? There are many ways to adjust their severity and even disable them altogether. For an overview of some of the options, check out [this document](https://docs.microsoft.com/en-gb/dotnet/fundamentals/code-analysis/suppress-warnings).