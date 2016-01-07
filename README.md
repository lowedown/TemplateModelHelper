# TemplateModelHelper
Helper class for working with generated Sitecore Template Models (Mapped through Glass.Mapper or other framework)

## Key features
* Easy to use, easy to read code
* Handles Template inheritance
* Unit-testable

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

Alternatively the helper can also be used as a class without extension methods:

    ITemplateModelHelper<IGlassBase> helper = new GlassTemplateModelHelper(new SitecoreContext());

    var children = helper.Children<ISomeTemplate>(myItem);

## Set-up

To use the TemplateModelHelper you need the following

* Code-Generation using TDS or an other tool already set up
* Generated interfaces have the TemplateModelHelper attribute
* Minimum IGlassBase

### Setting up code-generation using TDS

Add the TemplateModelHelper attribute in front of interfaces to the T4 Template.

    [TemplateModelHelper(TemplateId="<#= template.ID.ToString() #>",BaseTemplates = "<#=string.Join(",", template.BaseTemplates.Select(bt => bt.ID.ToString()).ToArray()) #>")]
	public partial interface <#= AsInterfaceName(template.Name) #>Template : IGlassBase <#=GetObjectInheritanceDefinition(DefaultNamespace, template, true, (string s) => AsInterfaceName(s) + "Template")#>
    {

### Minimum IGlassBase

To work correctly, TemplateModelHelper needs a IGlassBase interface with at least one attribute named "Item". All generated interfaces need to implement from this interface.

    public interface IGlassBase
    {
        [SitecoreItem]
        Item Item { get; }
    } 