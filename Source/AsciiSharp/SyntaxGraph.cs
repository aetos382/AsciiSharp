using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AsciiSharp;

public class SyntaxGraph :
    IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        yield break;
    }
}
