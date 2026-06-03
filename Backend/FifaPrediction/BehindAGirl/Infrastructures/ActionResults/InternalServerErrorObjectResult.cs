using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BehindAGirl.Infrastructures.ActionResults
{
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object error)
            : base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
