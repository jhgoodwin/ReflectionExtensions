# ReflectionExtensions
Library for doing interesting things using C# reflection

The idea was to create methods to support reflection in a way that allows you to do novel things.

First idea is an ExtendWith<T> method which adds an interface to a class.

Some uses of this idea was to be able to define interfaces as DTO contracts.

For example, create an assembly of interfaces, let consumers use them, but never have to define a concretion to hydrate them. With some work on defining the process and serialization system, this would do things like have an event bus, put message listeners for any interface you please, but potentially never have a concretion for the fully hydrated shape.

Take a look at the tests to see what already is supported.
