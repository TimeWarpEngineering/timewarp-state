// https://stackoverflow.com/questions/29470863/serilog-output-enrich-all-messages-with-methodname-from-which-log-entry-was-ca/46905798#46905798
// https://nblumhardt.com/2016/11/ilogger-beginscope/
// I don't really want to create new logger as the injected one is already created.
// instead maybe should use the "logger.BeginScope" to add the method name to the scope.
//namespace BlazorState.Extensions;

//using Microsoft.Extensions.Logging;
//using System.Runtime.CompilerServices;

//public static class LoggerExtensions
//{
//  public static ILogger Here(this ILogger logger,
//      [CallerMemberName] string memberName = "",
//      [CallerFilePath] string sourceFilePath = "",
//      [CallerLineNumber] int sourceLineNumber = 0)
//  {
//    return logger
//        .ForContext("MemberName", memberName)
//        .ForContext("FilePath", sourceFilePath)
//        .ForContext("LineNumber", sourceLineNumber);
//  }
//}
