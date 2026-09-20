using System.Management.Automation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace BuildJsonValidator.Commands;

    [Cmdlet(VerbsCommon.Get, "JsonSchemaValidation")]
    public class GetJsonSchemaValidationCommand : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string Json { get; set; }

        [Parameter(Mandatory = true)]
        public string Schema { get; set; }

        protected override void ProcessRecord()
        {
            var json = JObject.Parse(Json);
            var schema = JSchema.Parse(Schema);
            bool isValid = json.IsValid(schema, out IList<string> errorMessages);
            WriteObject(new { IsValid = isValid, Errors = errorMessages });
        }
    }
