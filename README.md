# TemplateModelHelper
Helper methods for working with generated Sitecore Template Models (Mapped through Glass.Mapper or other framework)

Main goal is to make it easy for developers to query the Sitecore database using generated Template models.

Read [this post](https://sitecoreblog.marklowe.ch/2016/01/querying-generated-template-models/ "this post") as introduction.

## Key features
* Easy to use, easy-to-read code
* Handles Template inheritance
* Unit-testing ready
* DI-ready

## Usage

To start, we need to map an Item to an Interface which inherits from IGlassBase.
	
    var myItem = new SitecoreContext().GetItem<IGlassBase>("/sitecore/content/GlassTest");

Now that we have a model implementing IGlassBase, we can use these simple extension methods	

    // Returns only children which inherit from the ISomeTemplate
    var theChildren = myItem.Children<ISomeTemplate>();

    // Returns only children which inherit from the ISomeOtherTemplate
    var otherChildren = myItem.Children<ISomeOtherTemplate>();

    // If I still want to get all child items, use the most generic interface
    var allChildren = myItem.Children<IGlassBase>();

    // Returns only descendants which inherit from ISomeTemplate
    var theDescendants = myItem.Descendants<ISomeTemplate>();

    // Returns only ancestors which inherit from the ISomeTemplate
    var ancestors = myItem.Ancestors<ISomeTemplate>();

    var parent = myItem.Parent<ISomeTemplate>();

    // Returns the first child inheriting from this template. More efficient than using myItem.Children<ISomeTemplate>.First()
    var firstChild = myItem.FirstChild<ISomeTemplate>();   

### Usage without extension methods

The helper can also be used as without extension methods:

    ITemplateModelHelper<IGlassBase> helper = new GlassTemplateModelHelper(new SitecoreContext());

    var children = helper.Children<ISomeTemplate>(myItem);

This allows you to add the helper class to a DI container and inject it wherever you need it. 

## Setup

To use the TemplateModelHelper you need the following

1. Code-generation using TDS or any other tool
2. Add TemplateModelHelper attribute to generated interfaces
3. Add reference to your IGlassBase and ensure, an Item attribute exists

### 1. Setting up code-generation using TDS

Code-generation using TDS and T4 templates needs to be up and running. See how to set that up here: [https://github.com/HedgehogDevelopment/tds-codegen/]().

### 2. Add TemplateModelHelper attribute to generated interfaces

Then, add the *TemplateModelHelper* attribute before interfaces to the T4 Template (i.E. : GlassVXItem.t4)

    [TemplateModelHelper(TemplateId="<#= template.ID.ToString() #>",BaseTemplates="<#=string.Join(",", template.BaseTemplates.Select(bt => bt.ID.ToString()).ToArray()) #>")]
	[SitecoreType(TemplateId=<#= AsInterfaceName(template.Name) #>Constants.TemplateIdString )] //, Cachable = true
	public partial interface <#= AsInterfaceName(template.Name) #> : IGlassBase <#=GetObjectInheritanceDefinition(DefaultNamespace, template, true, (string s) => AsInterfaceName(s))#>
	{

A generated interface should then look something like this:

    [SitecoreType(TemplateId=IMyTemplate.TemplateIdString)] // , Cachable = true
	[TemplateModelHelper(TemplateId="076616fe-123f-443d-b627-ff4c1da8df57",BaseTemplates="453e35fc-46c5-46b3-a447-141c103f9989,0b34a6eb-d5b6-4d59-b5f9-3ce0bcb13fdd")]
    public partial interface IMyTemplate : IGlassBase, IMyBaseTemplate, IMyOtherBaseTemplate
    {
	}

### 3. Add reference to your IGlassBase and ensure, an Item attribute exists

To work correctly, TemplateModelHelper needs a IGlassBase interface with at least one attribute named *Item*. All generated interfaces need to inherit from this interface.

    public interface IGlassBase
    {
        [SitecoreItem]
        Item Item { get; }

		//... your own attributes
    } 

When checking out the code you'll notice the missing reference to IGlassBase. Just point it at your own implementation.

## Unit Testing

If you need to test code using the helper's extension methods, you can replace the helper implementation through a mock like this:

    ITemplateModelHelper<IGlassBase> mock = new MyMockTemplateModelHelper();
    GlassTemplateModelExtensions.Helper = mock;

You could also extend the HelperFactory() in the GlassTemplateModelExtensions class to get the helper implementation from your DI container of choice.  

## Other OR Mappers

By implementing the *ITemplateModelHelper<T>* interface, you can also build implementations for other OR mappers than Glass.

## Known limitations

Currently, template inheritance is only supported with interfaces that reside in the same assembly. For example if IMyPageTemplate lives in *MyProject.MyModels.dll* and MySubPageTemplate which inherits *IMyPageTemplate* lives in *MyProject.MyOtherModels.dll*.

This shouldn't be a problem as long as your generated interfaces aren't spread across multiple assemblies.

Otherwise, this can be fixed by scanning all assemblies in *TemplateModelHelperAttribute.GetApplicapleTypes()* if needed.