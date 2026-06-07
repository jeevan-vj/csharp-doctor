# NDepend Default Rules Reference

Source: [NDepend Rules Explorer](https://www.ndepend.com/default-rules/NDepend-Rules-Explorer.html#!).

This document catalogs the default entries listed by the NDepend Rules Explorer. The source groups quality gates, CQLinq rules, trend metrics, code diff summaries, statistics, and custom-rule samples together, so this reference keeps the same structure instead of treating every entry as an executable code rule.

Total entries: **359**.

## Category Summary

- Quality Gates: 16
- Hot Spots: 7
- Code Smells: 10
- Code Smells Regression: 9
- Object Oriented Design: 14
- Design: 16
- Architecture: 13
- API Breaking Changes: 9
- Code Coverage: 14
- Dead Code: 4
- Security: 11
- Visibility: 12
- Immutability: 15
- Naming Conventions: 23
- Source Files Organization: 6
- .NET Framework Usage: 32
- API Usage: 9
- Defining JustMyCode: 7
- Trend Metrics: 78
- Code Diff Summary: 25
- Statistics: 13
- Samples of Custom rules: 16

## Quality Gates

Count: 16 entries.

- Quality Gates Evolution
- Percentage Coverage
- Percentage Coverage on New Code
- Percentage Coverage on Refactored Code
- Blocker Issues
- Critical Issues
- New Blocker / Critical / High Issues
- Critical Rules Violated
- Treat Compiler Warnings as Error
- Percentage Debt
- Debt
- New Debt since Baseline
- Debt Rating per Namespace
- Annual Interest
- New Annual Interest since Baseline
- Unnecessary NDepend SuppressMessageAttribute Usage

## Hot Spots

Count: 7 entries.

- Types Hot Spots
- Types to Fix Priority
- Issues to Fix Priority
- Debt and Issues per Rule
- New Debt and Issues per Rule
- Debt and Issues per Code Element
- New Debt and Issues per Code Element

## Code Smells

Count: 10 entries.

- Avoid types too big
- Avoid types with too many methods
- Avoid types with too many fields
- Avoid methods too big, too complex
- Avoid methods with too many parameters
- Avoid methods with too many overloads
- Avoid methods potentially poorly commented
- Avoid types with poor cohesion
- Avoid methods with too many local variables
- Avoid unmaintainable code

## Code Smells Regression

Count: 9 entries.

- From now, all types added should respect basic quality principles
- From now, all types added should be 100% covered by tests
- From now, all methods added should respect basic quality principles
- Avoid decreasing code coverage by tests of types
- Avoid making complex methods even more complex
- Avoid making large methods even larger
- Avoid adding methods to a type that already had many methods
- Avoid adding instance fields to a type that already had many instance fields
- Avoid transforming an immutable type into a mutable one

## Object Oriented Design

Count: 14 entries.

- Avoid interfaces too big
- Base class should not use derivatives
- Class shouldn't be too deep in inheritance tree
- Class with no descendant should be sealed if possible
- Overrides of Method() should call base.Method()
- Do not hide base class methods
- A stateless class or structure might be turned into a static type
- Non-static classes should be instantiated or turned to static
- Methods should be declared static if possible
- Constructor should not call a virtual method
- Avoid the Singleton pattern
- Don't assign static fields from instance methods
- Avoid empty interfaces
- Avoid types initialization cycles

## Design

Count: 16 entries.

- Avoid custom delegates
- Types with disposable instance fields must be disposable
- Disposable types with unmanaged resources should declare finalizer
- Methods that create disposable object(s) and that don't call Dispose()
- Disposable Types Must Unsubscribe Events
- Classes that are candidate to be turned into structures
- Avoid namespaces with few types
- Nested types should not be visible
- Declare types in namespaces
- Empty static constructor can be discarded
- Instances size shouldn't be too big
- Attribute classes should be sealed
- Don't use obsolete types, methods or fields
- Do implement methods that throw NotImplementedException
- Override equals and operator equals on value types
- Boxing/unboxing should be avoided

## Architecture

Count: 13 entries.

- Avoid namespaces mutually dependent
- Avoid namespaces dependency cycles
- Avoid partitioning the code base through many small library Assemblies
- Enforcing Clean Architecture
- UI layer shouldn't use directly DB types
- UI layer shouldn't use directly DAL layer
- Assemblies with poor cohesion (RelationalCohesion)
- Namespaces with poor cohesion (RelationalCohesion)
- Assemblies that don't satisfy the Abstractness/Instability principle
- Avoid excessive class coupling
- Higher cohesion - lower coupling
- Avoid mutually-dependent types
- Example of custom rule to check for dependency

## API Breaking Changes

Count: 9 entries.

- API Breaking Changes: Types
- API Breaking Changes: Methods
- API Breaking Changes: Fields
- API Breaking Changes: Interfaces and Abstract Classes
- Broken serializable types
- Avoid changing enumerations Flags status
- API: New publicly visible types
- API: New publicly visible methods
- API: New publicly visible fields

## Code Coverage

Count: 14 entries.

- Code should be tested
- New Types and Methods should be tested
- Methods refactored should be tested
- Assemblies and Namespaces should be tested
- Types almost 100% tested should be 100% tested
- Namespaces almost 100% tested should be 100% tested
- Types that used to be 100% covered by tests should still be 100% covered
- Types tagged with FullCoveredAttribute should be 100% covered
- Types 100% covered should be tagged with FullCoveredAttribute
- Methods should have a low C.R.A.P score
- Complex Methods should be 100% tested
- Test Methods
- Methods directly called by test Methods
- Methods directly and indirectly called by test Methods

## Dead Code

Count: 4 entries.

- Potentially Dead Types
- Potentially Dead Methods
- Potentially Dead Fields
- Wrong usage of IsNotDeadCodeAttribute

## Security

Count: 11 entries.

- Software Composition Analysis (SCA)
- Transitive Software Composition Analysis (SCA)
- Don't use CoSetProxyBlanket and CoInitializeSecurity
- Don't use System.Random for security purposes
- Don't use DES/3DES weak cipher algorithms
- Don't disable certificate validation
- Review publicly visible event handlers
- Pointers should not be publicly visible
- Seal methods that satisfy non-public interfaces
- Review commands vulnerable to SQL injection
- Review data adapters vulnerable to SQL injection

## Visibility

Count: 12 entries.

- Methods that could have a lower visibility
- Types that could have a lower visibility
- Fields that could have a lower visibility
- Types that could be declared as private, nested in a parent type
- Avoid publicly visible constant fields
- Fields should be declared as private or protected
- Constructors of abstract classes should be declared as protected or private
- Avoid public methods not publicly visible
- Event handler methods should be declared as private or protected
- Wrong usage of CannotDecreaseVisibilityAttribute
- Exception classes should be declared as public
- Methods that should be declared as 'public' in C#, 'Public' in VB.NET

## Immutability

Count: 15 entries.

- Fields should be marked as ReadOnly when possible
- Avoid non-readonly static fields
- Avoid static fields with a mutable field type
- Structures should be immutable
- Immutable struct should be declared as readonly
- Property Getters should be pure
- A field must not be assigned from outside its parent hierarchy types
- Don't assign a field from many methods
- Do not declare read only fields with mutable reference types
- Public read only array fields can be modified
- Types tagged with ImmutableAttribute must be immutable
- Types immutable should be tagged with ImmutableAttribute
- Methods tagged with PureAttribute must be pure
- Pure methods should be tagged with PureAttribute
- Record should be immutable

## Naming Conventions

Count: 23 entries.

- Instance fields naming convention
- Static fields naming convention
- Interface name should begin with a 'I'
- Abstract base class should be suffixed with 'Base'
- Exception class name should be suffixed with 'Exception'
- Attribute class name should be suffixed with 'Attribute'
- Types name should begin with an Upper character
- Methods name should begin with an Upper character
- Do not name enum values 'Reserved'
- Avoid types with name too long
- Avoid methods with name too long
- Avoid fields with name too long
- Avoid having different types with same name
- Avoid prefixing type name with parent namespace name
- Avoid naming types and namespaces with the same identifier
- Don't call your method Dispose
- Methods prefixed with 'Try' should return a boolean
- Properties and fields that represent a collection of items should be named Items.
- DDD ubiquitous language check
- Avoid fields with same name in class hierarchy
- Avoid various capitalizations for method name
- Controller class name should be suffixed with 'Controller'
- Nested class members should not mask outer class' static members

## Source Files Organization

Count: 6 entries.

- Avoid referencing source file out of the project directory
- Avoid duplicating a type definition across assemblies
- Avoid defining multiple types in a source file
- Namespace name should correspond to file location
- Types with source files stored in the same directory, should be declared in the same namespace
- Types declared in the same namespace, should have their source files stored in the same directory

## .NET Framework Usage

### System

Count: 12 entries.

- Mark ISerializable types with SerializableAttribute
- Mark assemblies with CLSCompliant (deprecated)
- Mark assemblies with ComVisible (deprecated)
- Mark attributes with AttributeUsageAttribute
- Remove calls to GC.Collect()
- Don't call GC.Collect() without calling GC.WaitForPendingFinalizers()
- Enum Storage should be Int32
- Do not raise too general exception types
- Do not raise reserved exception types
- Uri fields or properties should be of type System.Uri
- Types should not derive from System.ApplicationException
- Don't Implement ICloneable

### System.Collections

Count: 4 entries.

- Collection properties should be read only
- Don't use .NET 1.x HashTable and ArrayList
- Caution with List.Contains()
- Prefer return collection abstraction instead of implementation

### System.Runtime.InteropServices

Count: 3 entries.

- P/Invokes should be static and not be publicly visible
- Move P/Invokes to NativeMethods class
- NativeMethods class should be static and internal

### System.Threading

Count: 6 entries.

- Don't create threads explicitly
- Don't use dangerous threading methods
- Monitor TryEnter/Exit must be both called within the same method
- ReaderWriterLock AcquireLock/ReleaseLock must be both called within the same method
- Don't tag instance fields with ThreadStaticAttribute
- Method non-synchronized that read mutable states

### System.Xml

Count: 2 entries.

- Method should not return concrete XmlNode
- Types should not derive from System.Xml.XmlDocument

### System.Globalization

Count: 1 entry.

- Float and Date Parsing must be culture aware

### System.Reflection

Count: 3 entries.

- Mark assemblies with assembly version
- Assemblies should have the same version
- Assemblies Referenced in Multiple Versions

### Microsoft.Contracts

Count: 1 entry.

- Public methods returning a reference needs a contract to ensure that a non-null reference is returned

## API Usage

### Unity

Count: 9 entries.

- Classes tagged with InitializeOnLoad should have a static constructor
- Avoid using non-generic GetComponent
- Avoid empty Unity message
- Avoid using Time.fixedDeltaTime with Update
- Use CreateInstance to create a scriptable object
- The SerializeField attribute is redundant on public fields
- InitializeOnLoadMethod should tag only static and parameterless methods
- Prefer using SetPixels32() over SetPixels()
- Don't use System.Reflection in performance critical messages

## Defining JustMyCode

Count: 7 entries.

- Discard generated Assemblies from JustMyCode
- Discard generated Namespaces from JustMyCode
- Discard generated Types from JustMyCode
- Discard generated and designer Methods from JustMyCode
- Discard generated Fields from JustMyCode
- JustMyCode code elements
- NotMyCode code elements

## Trend Metrics

### Issues

Count: 11 entries.

- # New Issues since Baseline
- # Issues Fixed since Baseline
- # Issues Worsened since Baseline
- # Issues with severity Blocker
- # Issues with severity Critical
- # Issues with severity High
- # Issues with severity Medium
- # Issues with severity Low
- # Blocker/Critical/High Issues
- # Issues
- # Suppressed Issues

### Rules

Count: 3 entries.

- # Rules
- # Rules Violated
- # Critical Rules Violated

### Quality Gates

Count: 3 entries.

- # Quality Gates
- # Quality Gates Warn
- # Quality Gates Fail

### Debt

Count: 7 entries.

- Percentage Debt (Metric)
- Debt (Metric)
- New Debt since Baseline (Metric)
- Annual Interest (Metric)
- New Annual Interest since Baseline (Metric)
- Breaking Point
- Breaking Point of Blocker / Critical / High Issues

### Code Size

Count: 22 entries.

- # Lines of Code
- # Lines of Code (JustMyCode)
- # Lines of Code (NotMyCode)
- # Lines of Code Added since the Baseline
- # Source Files
- # Line Feed
- # IL Instructions
- # IL Instructions (NotMyCode)
- # Lines of Comments
- Percentage of Comments
- # Assemblies
- # Namespaces
- # Types
- # Public Types
- # Classes
- # Abstract Classes
- # Interfaces
- # Structures
- # Methods
- # Abstract Methods
- # Concrete Methods
- # Fields

### Maximum and Average

Count: 15 entries.

- Max # Lines of Code for Methods (JustMyCode)
- Average # Lines of Code for Methods
- Average # Lines of Code for Methods with at least 3 Lines of Code
- Max # Lines of Code for Types (JustMyCode)
- Average # Lines of Code for Types
- Max Cyclomatic Complexity for Methods
- Average Cyclomatic Complexity for Methods
- Max IL Cyclomatic Complexity for Methods
- Average IL Cyclomatic Complexity for Methods
- Max IL Nesting Depth for Methods
- Average IL Nesting Depth for Methods
- Max # of Methods for Types
- Average # Methods for Types
- Max # of Methods for Interfaces
- Average # Methods for Interfaces

### Coverage

Count: 11 entries.

- Percentage Code Coverage
- # Lines of Code Covered
- # Lines of Code Not Covered
- # Lines of Code Uncoverable
- # Lines of Code in Types 100% Covered
- # Lines of Code in Methods 100% Covered
- Max C.R.A.P Score
- Average C.R.A.P Score
- Percentage JustMyCode Coverage
- # Lines of JustMyCode Covered
- # Lines of JustMyCode Not Covered

### Third-Party Usage

Count: 6 entries.

- # Third-Party Assemblies Used
- # Third-Party Namespaces Used
- # Third-Party Types Used
- # Third-Party Methods Used
- # Third-Party Fields Used
- # Third-Party Code Elements Used

## Code Diff Summary

Count: 25 entries.

- New assemblies
- Assemblies removed
- Assemblies where code was changed
- New namespaces
- Namespaces removed
- Namespaces where code was changed
- New types
- Types removed
- Types where code was changed
- Heuristic to find types moved from one namespace or assembly to another
- Types directly using one or several types changed
- Types indirectly using one or several types changed
- New methods
- Methods removed
- Methods where code was changed
- Methods directly calling one or several methods changed
- Methods indirectly calling one or several methods changed
- New fields
- Fields removed
- Third party types that were not used and that are now used
- Third party types that were used and that are not used anymore
- Third party methods that were not used and that are now used
- Third party methods that were used and that are not used anymore
- Third party fields that were not used and that are now used
- Third party fields that were used and that are not used anymore

## Statistics

Count: 13 entries.

- Most used types (Rank)
- Most used methods (Rank)
- Most used assemblies (#AssembliesUsingMe)
- Most used namespaces (#NamespacesUsingMe )
- Most used types (#TypesUsingMe )
- Most used methods (#MethodsCallingMe )
- Namespaces that use many other namespaces (#NamespacesUsed )
- Types that use many other types (#TypesUsed )
- Methods that use many other methods (#MethodsCalled )
- High-level to low-level assemblies (Level)
- High-level to low-level namespaces (Level)
- High-level to low-level types (Level)
- High-level to low-level methods (Level)

## Samples of Custom rules

### Check Architecture

Count: 10 entries.

- Check that the assembly Asm1 is not using the assembly Asm2
- Check that the namespace N1.N2 is not using the namespace N3.N4.N5
- Check that the assembly Asm1 is only using the assemblies Asm2, Asm3 or System.Runtime
- Check that the namespace N1.N2 is only using the namespaces N3.N4, N5 or System
- Check that AsmABC is the only assembly that is using System.Collections.Concurrent
- Check that only 3 assemblies are using System.Collections.Concurrent
- Check that all methods that call Foo.Fct1() also call Foo.Fct2(Int32)
- Check that all types that derive from Foo, also implement IFoo
- Check that all types that has the attribute FooAttribute are declared in the namespace N1.N2\*
- Check that all synchronization objects are only used from the namespaces under MyNamespace.Sync

### Check Coverage on particular Code Elements

Count: 4 entries.

- Check that the namespace N1.N2 is 100% covered by tests
- Check that the assembly Asm is 100% covered by tests
- Check that the class Namespace.Foo is 100% covered by tests
- Check that the method Namespace.Foo.Method(Int32) is 100% covered by tests

### Custom Naming Conventions

Count: 2 entries.

- Check that all types that derive from Foo, has a name that ends up with Foo
- Check that all namespaces begins with CompanyName.ProductName
