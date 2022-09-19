namespace SharpSource.Utilities;
public static class DiagnosticId
{
    public const string AsyncMethodWithVoidReturnType = "SS001";
    public const string DateTimeNow = "SS002";
    public const string DivideIntegerByInteger = "SS003";
    public const string ElementaryMethodsOfTypeInCollectionNotOverridden = "SS004";
    public const string EqualsAndGetHashcodeNotImplementedTogether = "SS005";
    public const string ThrowNull = "SS006";
    public const string FlagsEnumValuesAreNotPowersOfTwo = "SS007";
    public const string GetHashCodeRefersToMutableMember = "SS008";
    public const string LoopedRandomInstantiation = "SS009";
    public const string NewGuid = "SS010";
    public const string OnPropertyChangedWithoutNameofOperator = "SS011";
    public const string RecursiveOperatorOverload = "SS012";
    public const string RethrowExceptionWithoutLosingStacktrace = "SS013";
    public const string StringDotFormatWithDifferentAmountOfArguments = "SS014";
    public const string StringPlaceholdersInWrongOrder = "SS015";
    public const string StructWithoutElementaryMethodsOverridden = "SS017";
    public const string SwitchDoesNotHandleAllEnumOptions = "SS018";
    public const string SwitchIsMissingDefaultLabel = "SS019";
    public const string TestMethodWithoutPublicModifier = "SS020";
    public const string TestMethodWithoutTestAttribute = "SS021";
    public const string ExceptionThrownFromImplicitOperator = "SS022";
    public const string ExceptionThrownFromPropertyGetter = "SS023";
    public const string ExceptionThrownFromStaticConstructor = "SS024";
    public const string ExceptionThrownFromFinallyBlock = "SS025";
    public const string ExceptionThrownFromEqualityOperator = "SS026";
    public const string ExceptionThrownFromDispose = "SS027";
    public const string ExceptionThrownFromFinalizer = "SS028";
    public const string ExceptionThrownFromGetHashCode = "SS029";
    public const string ExceptionThrownFromEquals = "SS030";
    public const string ThreadSleepInAsyncMethod = "SS032";
    public const string AsyncOverloadsAvailable = "SS033";
    public const string AccessingTaskResultWithoutAwait = "SS034";
    public const string SynchronousTaskWait = "SS035";
    public const string ExplicitEnumValues = "SS036";
    public const string HttpClientInstantiatedDirectly = "SS037";
    public const string HttpContextStoredInField = "SS038";
    public const string EnumWithoutDefaultValue = "SS039";
    public const string UnusedResultOnImmutableObject = "SS040";
    public const string UnnecessaryEnumerableMaterialization = "SS041";
    public const string InstanceFieldWithThreadStatic = "SS042";
    public const string MultipleFromBodyParameters = "SS043";
    public const string AttributeMustSpecifyAttributeUsage = "SS044";
    public const string StaticInitializerAccessedBeforeInitialization = "SS045";
    public const string UnboundedStackalloc = "SS046";
    public const string LinqTraversalBeforeFilter = "SS047";
    public const string LockingOnDiscouragedObject = "SS048";
    public const string ComparingStringsWithoutStringComparison = "SS049";
}