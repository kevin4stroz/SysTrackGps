using System;

namespace SysTrackGps.Utilities;

public class GenCodeErrorResponse
{
    /// <summary>
    /// Genera codigo unico de excepcion para control post error
    /// </summary>
    /// <returns></returns>
    public static string BuildErrorCode()
    {
        DateTime now = DateTime.Now;
        string month = now.Month > 9 ? $"{now.Month}" : $"0{now.Month}";
        string day = now.Day > 9 ? $"{now.Day}" : $"0{now.Day}";
        string hour = now.Hour > 9 ? $"{now.Hour}" : $"0{now.Hour}";
        string minute = now.Minute > 9 ? $"{now.Minute}" : $"0{now.Minute}";
        string second = now.Second > 9 ? $"{now.Second}" : $"0{now.Second}";
        long rnd = GenerateRandomNumber(2);

        return $"{month}{day}{hour}{minute}{second}{rnd}";
    }

    /// <summary>
    /// Genera un numero aleatorio de cierto tamaño
    /// </summary>
    /// <param name="characters"></param>
    /// <returns></returns>
    public static long GenerateRandomNumber(int characters)
    {
        Random random = new Random();

        const string chars = "123456789";

        return Convert
            .ToInt64(new string(Enumerable.Repeat(chars, characters)
            .Select(s => s[random.Next(s.Length)])
            .ToArray()));
    }

}
