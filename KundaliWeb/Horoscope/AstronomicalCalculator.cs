using SwissEphNet;
using VedAstro.Library;

namespace VedAstro.Library;

public static class AstronomicalCalculator
{
    public static Angle TimeToLongitude(TimeSpan time)
    {
        double value = time.TotalHours * 15.0;
        return Angle.FromDegrees(value);
    }

    public static double TimeToEphemerisTime(Time time)
    {
        SwissEph swissEph = new SwissEph();
        int gregflag = 1;
        DateTimeOffset dateTimeOffset = LmtToUtc(time);
        int year = dateTimeOffset.Year;
        int month = dateTimeOffset.Month;
        int day = dateTimeOffset.Day;
        double totalHours = dateTimeOffset.TimeOfDay.TotalHours;
        double num = swissEph.swe_julday(year, month, day, totalHours, gregflag);
        return num + swissEph.swe_deltat(num);
    }

    public static Angle GetPlanetNirayanaLongitude(Time time, PlanetName planetName)
    {
        Angle planetSayanaLongitude = GetPlanetSayanaLongitude(time, planetName);
        Angle ayanamsa = GetAyanamsa(time);
        Angle angle = ((planetSayanaLongitude.TotalDegrees < ayanamsa.TotalDegrees) ?
        (planetSayanaLongitude + Angle.Degrees360 - ayanamsa) : (planetSayanaLongitude - ayanamsa));
        if (planetName == PlanetName.Ketu)
        {
            angle = ((angle < Angle.Degrees180) ? (Angle.Degrees180 - angle) : (angle - Angle.Degrees180));
        }

        return angle;
    }

    public static LunarDay GetLunarDay(Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, PlanetName.Sun);
        Angle planetNirayanaLongitude2 = GetPlanetNirayanaLongitude(time, PlanetName.Moon);
        double a = ((!(planetNirayanaLongitude2.TotalDegrees > planetNirayanaLongitude.TotalDegrees)) ?
            ((planetNirayanaLongitude2 + Angle.Degrees360 - planetNirayanaLongitude).TotalDegrees / 12.0)
            : ((planetNirayanaLongitude2 - planetNirayanaLongitude).TotalDegrees / 12.0));
        int lunarDateNumber = (int)Math.Ceiling(a);
        return new LunarDay(lunarDateNumber);
    }

    public static PlanetConstellation GetMoonConstellation(Time time)
    {
        return GetPlanetConstellation(time, PlanetName.Moon);
    }

    public static PlanetConstellation GetPlanetConstellation(Time time, PlanetName planet)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planet);
        return GetConstellationAtLongitude(planetNirayanaLongitude);
    }

    public static Tarabala GetTarabala(Time time, Person person)
    {
        int constellationNumber = GetMoonConstellation(time).GetConstellationNumber();
        int constellationNumber2 = GetMoonConstellation(person.BirthTime).GetConstellationNumber();
        int num = 0;
        if (constellationNumber2 > constellationNumber)
        {
            int num2 = 27 - constellationNumber2 + 1;
            num = constellationNumber + num2;
        }
        else if (constellationNumber2 == constellationNumber)
        {
            num = 1;
        }
        else if (constellationNumber2 < constellationNumber)
        {
            num = constellationNumber - constellationNumber2 + 1;
        }

        int cycle = (int)Math.Ceiling((double)num / 9.0);
        if (num > 9)
        {
            num %= 9;
            if (num == 0)
            {
                num = 9;
            }
        }

        return new Tarabala(num, cycle);
    }

    public static int GetChandrabala(Time time, Person person)
    {
        int result = 0;
        int moonSignName = (int)GetMoonSignName(time);
        int moonSignName2 = (int)GetMoonSignName(person.BirthTime);
        if (moonSignName2 > moonSignName)
        {
            int num = 12 - moonSignName2 + 1;
            result = moonSignName + num;
        }
        else if (moonSignName2 == moonSignName)
        {
            result = 1;
        }
        else if (moonSignName2 < moonSignName)
        {
            result = moonSignName - moonSignName2 + 1;
        }

        return result;
    }

    public static ZodiacName GetMoonSignName(Time time)
    {
        return GetPlanetRasiSign(PlanetName.Moon, time).GetSignName();
    }

    public static NithyaYogaName GetNithyaYoga(Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, PlanetName.Sun);
        Angle planetNirayanaLongitude2 = GetPlanetNirayanaLongitude(time, PlanetName.Moon);
        double num = planetNirayanaLongitude.TotalMinutes + planetNirayanaLongitude2.TotalMinutes;
        double a = num / 800.0;
        double num2 = Math.Ceiling(a);
        return (NithyaYogaName)num2;
    }

    public static Karana GetKarana(Time time)
    {
        Karana? karana = null;
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, PlanetName.Sun);
        Angle planetNirayanaLongitude2 = GetPlanetNirayanaLongitude(time, PlanetName.Moon);
        double num = ((!(planetNirayanaLongitude2.TotalDegrees > planetNirayanaLongitude.TotalDegrees)) ? ((planetNirayanaLongitude2 + new Angle(360.0, 0.0, 0L) - planetNirayanaLongitude).TotalDegrees / 12.0) : ((planetNirayanaLongitude2 - planetNirayanaLongitude).TotalDegrees / 12.0));
        int num2 = (int)Math.Ceiling(num);
        double num3 = num - Math.Floor(num);
        switch (num2)
        {
            case 1:
                karana = ((num3 <= 0.5) ? Karana.Kimstughna : Karana.Bava);
                break;
            case 2:
            case 9:
            case 16:
            case 23:
                karana = ((num3 <= 0.5) ? Karana.Balava : Karana.Kaulava);
                break;
            case 3:
            case 10:
            case 17:
            case 24:
                karana = ((num3 <= 0.5) ? Karana.Taitula : Karana.Garija);
                break;
            case 4:
            case 11:
            case 18:
            case 25:
                karana = ((num3 <= 0.5) ? Karana.Vanija : Karana.Visti);
                break;
            case 5:
            case 12:
            case 19:
            case 26:
                karana = ((!(num3 <= 0.5)) ? Karana.Balava : Karana.Bava);
                break;
            case 6:
            case 13:
            case 20:
            case 27:
                karana = ((num3 <= 0.5) ? Karana.Kaulava : Karana.Taitula);
                break;
            case 7:
            case 14:
            case 21:
            case 28:
                karana = ((num3 <= 0.5) ? Karana.Garija : Karana.Vanija);
                break;
            case 8:
            case 15:
            case 22:
                karana = ((num3 <= 0.5) ? Karana.Visti : Karana.Bava);
                break;
            case 29:
                karana = ((num3 <= 0.5) ? Karana.Visti : Karana.Sakuna);
                break;
            case 30:
                karana = ((num3 <= 0.5) ? Karana.Chatushpada : Karana.Naga);
                break;
        }

        if (!karana.HasValue)
        {
            throw new Exception("Karana could not be found!");
        }

        return karana.Value;
    }

    public static ZodiacSign GetSunSign(Time time)
    {
        return GetPlanetRasiSign(PlanetName.Sun, time);
    }

    public static Time GetTimeSunEnteredCurrentSign(Time time)
    {
        double minute = TimePreset.Minute3;
        double num = 96.0;
        Time time2 = time;
        Time time3 = time;
        ZodiacSign sunSign = GetSunSign(time);
        while (true)
        {
            ZodiacSign sunSign2 = GetSunSign(time2);
            if (sunSign2.GetSignName() == sunSign.GetSignName())
            {
                if (sunSign2.GetDegreesInSign().TotalDegrees < 0.001)
                {
                    break;
                }

                time3 = time2;
                time2 = time2.SubtractHours(num);
            }
            else
            {
                time2 = time3;
                if (num <= minute)
                {
                    break;
                }

                num /= 2.0;
            }
        }

        return time2;
    }

    public static Time GetTimeSunLeavesCurrentSign(Time time)
    {
        double minute = TimePreset.Minute3;
        double num = 96.0;
        Time time2 = time;
        Time time3 = time;
        ZodiacSign sunSign = GetSunSign(time);
        while (true)
        {
            ZodiacSign sunSign2 = GetSunSign(time2);
            if (sunSign2.GetSignName() == sunSign.GetSignName())
            {
                if (sunSign2.GetDegreesInSign().TotalDegrees > 29.999)
                {
                    break;
                }

                time3 = time2;
                time2 = time2.AddHours(num);
            }
            else
            {
                time2 = time3;
                if (num <= minute)
                {
                    break;
                }

                num /= 2.0;
            }
        }

        return time2;
    }

    public static List<House> GetHouses(Time time)
    {
        double[] house1And10Longitudes = GetHouse1And10Longitudes(time);
        Angle angle = Angle.FromDegrees(house1And10Longitudes[1]);
        Angle angle2 = Angle.FromDegrees(house1And10Longitudes[10]);
        Angle ayanamsa = GetAyanamsa(time);
        Angle angle3 = angle - ayanamsa;
        Angle angle4 = angle2 - ayanamsa;
        Angle angle5 = angle3 + Angle.Degrees180;
        Angle angle6 = angle4 + Angle.Degrees180;
        angle5 = angle5.Expunge360();
        angle6 = angle6.Expunge360();
        Angle angle7 = angle3;
        Angle angle8 = angle6;
        Angle angle9 = angle5;
        Angle angle10 = angle4;
        Angle angle11 = ((!(angle8 < angle7)) ? (angle8 - angle7) : (angle8 + Angle.Degrees360 - angle7));
        Angle angle12 = ((!(angle9 < angle8)) ? (angle9 - angle8) : (angle9 + Angle.Degrees360 - angle8));
        Angle angle13 = ((!(angle10 < angle9)) ? (angle10 - angle9) : (angle10 + Angle.Degrees360 - angle9));
        Angle angle14 = ((!(angle7 < angle10)) ? (angle7 - angle10) : (angle7 + Angle.Degrees360 - angle10));
        Angle angle15 = angle7 + angle11.Divide(3.0);
        angle15 = angle15.Expunge360();
        Angle angle16 = angle15 + angle11.Divide(3.0);
        angle16 = angle16.Expunge360();
        Angle angle17 = angle8 + angle12.Divide(3.0);
        angle17 = angle17.Expunge360();
        Angle angle18 = angle17 + angle12.Divide(3.0);
        angle18 = angle18.Expunge360();
        Angle angle19 = angle9 + angle13.Divide(3.0);
        angle19 = angle19.Expunge360();
        Angle angle20 = angle19 + angle13.Divide(3.0);
        angle20 = angle20.Expunge360();
        Angle angle21 = angle10 + angle14.Divide(3.0);
        angle21 = angle21.Expunge360();
        Angle angle22 = angle21 + angle14.Divide(3.0);
        angle22 = angle22.Expunge360();
        Angle houseJunctionPoint;
        Angle endLongitude = (houseJunctionPoint = GetHouseJunctionPoint(angle7, angle15));
        Angle houseJunctionPoint2;
        Angle endLongitude2 = (houseJunctionPoint2 = GetHouseJunctionPoint(angle15, angle16));
        Angle houseJunctionPoint3;
        Angle endLongitude3 = (houseJunctionPoint3 = GetHouseJunctionPoint(angle16, angle8));
        Angle houseJunctionPoint4;
        Angle endLongitude4 = (houseJunctionPoint4 = GetHouseJunctionPoint(angle8, angle17));
        Angle houseJunctionPoint5;
        Angle endLongitude5 = (houseJunctionPoint5 = GetHouseJunctionPoint(angle17, angle18));
        Angle houseJunctionPoint6;
        Angle endLongitude6 = (houseJunctionPoint6 = GetHouseJunctionPoint(angle18, angle9));
        Angle houseJunctionPoint7;
        Angle endLongitude7 = (houseJunctionPoint7 = GetHouseJunctionPoint(angle9, angle19));
        Angle houseJunctionPoint8;
        Angle endLongitude8 = (houseJunctionPoint8 = GetHouseJunctionPoint(angle19, angle20));
        Angle houseJunctionPoint9;
        Angle endLongitude9 = (houseJunctionPoint9 = GetHouseJunctionPoint(angle20, angle10));
        Angle houseJunctionPoint10;
        Angle endLongitude10 = (houseJunctionPoint10 = GetHouseJunctionPoint(angle10, angle21));
        Angle houseJunctionPoint11;
        Angle endLongitude11 = (houseJunctionPoint11 = GetHouseJunctionPoint(angle21, angle22));
        Angle houseJunctionPoint12;
        Angle endLongitude12 = (houseJunctionPoint12 = GetHouseJunctionPoint(angle22, angle7));
        return new List<House>
            {
                new House(1, houseJunctionPoint12, angle7, endLongitude),
                new House(2, houseJunctionPoint, angle15, endLongitude2),
                new House(3, houseJunctionPoint2, angle16, endLongitude3),
                new House(4, houseJunctionPoint3, angle8, endLongitude4),
                new House(5, houseJunctionPoint4, angle17, endLongitude5),
                new House(6, houseJunctionPoint5, angle18, endLongitude6),
                new House(7, houseJunctionPoint6, angle9, endLongitude7),
                new House(8, houseJunctionPoint7, angle19, endLongitude8),
                new House(9, houseJunctionPoint8, angle20, endLongitude9),
                new House(10, houseJunctionPoint9, angle10, endLongitude10),
                new House(11, houseJunctionPoint10, angle21, endLongitude11),
                new House(12, houseJunctionPoint11, angle22, endLongitude12)
            };
    }

    public static double TimeToJulianDay(Time time)
    {
        DateTimeOffset dateTimeOffset = time.GetLmtDateTimeOffset().ToUniversalTime();
        SwissEph swissEph = new SwissEph();
        return swissEph.swe_julday(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.TimeOfDay.TotalHours, 1);
    }

    public static List<PlanetName> GetPlanetsInConjuction(Time time, PlanetName inputedPlanetName)
    {
        Angle angle = new Angle(8.0, 0.0, 0L);
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, inputedPlanetName);
        List<PlanetLongitude> allPlanetLongitude = GetAllPlanetLongitude(time);
        List<PlanetName> list = new List<PlanetName>();
        foreach (PlanetLongitude item in allPlanetLongitude)
        {
            if (!(item.GetPlanetName() == inputedPlanetName))
            {
                Angle distanceBetweenPlanets = GetDistanceBetweenPlanets(planetNirayanaLongitude, item.GetPlanetLongitude());
                if (distanceBetweenPlanets >= Angle.Zero && distanceBetweenPlanets <= angle)
                {
                    list.Add(item.GetPlanetName());
                }
            }
        }

        return list;
    }

    public static Angle GetDistanceBetweenPlanets(PlanetName planet1, PlanetName planet2, Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planet1);
        Angle planetNirayanaLongitude2 = GetPlanetNirayanaLongitude(time, planet2);
        double value = planetDistance(planetNirayanaLongitude.TotalDegrees, planetNirayanaLongitude2.TotalDegrees);
        return Angle.FromDegrees(value);
        static double a_red(double x, double a)
        {
            return x - Math.Floor(x / a) * a;
        }

        static double planetDistance(double len1, double len2)
        {
            double num = red_deg(Math.Abs(len2 - len1));
            if (num > 180.0)
            {
                return 360.0 - num;
            }

            return num;
        }

        static double red_deg(double input)
        {
            return a_red(input, 360.0);
        }
    }

    public static Angle GetDistanceBetweenPlanets(Angle planet1, Angle planet2)
    {
        double value = planetDistance(planet1.TotalDegrees, planet2.TotalDegrees);
        return Angle.FromDegrees(value);
        static double a_red(double x, double a)
        {
            return x - Math.Floor(x / a) * a;
        }

        static double planetDistance(double len1, double len2)
        {
            double num = red_deg(Math.Abs(len2 - len1));
            if (num > 180.0)
            {
                return 360.0 - num;
            }

            return num;
        }

        static double red_deg(double input)
        {
            return a_red(input, 360.0);
        }
    }

    public static List<PlanetName> GetPlanetsInHouse(int houseNumber, Time time)
    {
        List<PlanetName> list = new List<PlanetName>();
        List<House> houses = GetHouses(time);
        House house = houses.Find((House h) => h.GetHouseNumber() == houseNumber);
        List<PlanetLongitude> allPlanetLongitude = GetAllPlanetLongitude(time);
        foreach (PlanetLongitude item in allPlanetLongitude)
        {
            if (house.IsLongitudeInHouseRange(item.GetPlanetLongitude()))
            {
                list.Add(item.GetPlanetName());
            }
        }

        return list;
    }

    public static List<PlanetLongitude> GetAllPlanetLongitude(Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, PlanetName.Sun);
        PlanetLongitude item = new PlanetLongitude(PlanetName.Sun, planetNirayanaLongitude);
        Angle planetNirayanaLongitude2 = GetPlanetNirayanaLongitude(time, PlanetName.Moon);
        PlanetLongitude item2 = new PlanetLongitude(PlanetName.Moon, planetNirayanaLongitude2);
        Angle planetNirayanaLongitude3 = GetPlanetNirayanaLongitude(time, PlanetName.Mars);
        PlanetLongitude item3 = new PlanetLongitude(PlanetName.Mars, planetNirayanaLongitude3);
        Angle planetNirayanaLongitude4 = GetPlanetNirayanaLongitude(time, PlanetName.Mercury);
        PlanetLongitude item4 = new PlanetLongitude(PlanetName.Mercury, planetNirayanaLongitude4);
        Angle planetNirayanaLongitude5 = GetPlanetNirayanaLongitude(time, PlanetName.Jupiter);
        PlanetLongitude item5 = new PlanetLongitude(PlanetName.Jupiter, planetNirayanaLongitude5);
        Angle planetNirayanaLongitude6 = GetPlanetNirayanaLongitude(time, PlanetName.Venus);
        PlanetLongitude item6 = new PlanetLongitude(PlanetName.Venus, planetNirayanaLongitude6);
        Angle planetNirayanaLongitude7 = GetPlanetNirayanaLongitude(time, PlanetName.Saturn);
        PlanetLongitude item7 = new PlanetLongitude(PlanetName.Saturn, planetNirayanaLongitude7);
        Angle planetNirayanaLongitude8 = GetPlanetNirayanaLongitude(time, PlanetName.Rahu);
        PlanetLongitude item8 = new PlanetLongitude(PlanetName.Rahu, planetNirayanaLongitude8);
        Angle planetNirayanaLongitude9 = GetPlanetNirayanaLongitude(time, PlanetName.Ketu);
        PlanetLongitude item9 = new PlanetLongitude(PlanetName.Ketu, planetNirayanaLongitude9);
        return new List<PlanetLongitude> { item, item2, item3, item4, item5, item6, item7, item9, item8 };
    }

    public static List<PlanetLongitude> GetAllPlanetFixedLongitude(Time time)
    {
        Angle planetSayanaLongitude = GetPlanetSayanaLongitude(time, PlanetName.Sun);
        PlanetLongitude item = new PlanetLongitude(PlanetName.Sun, planetSayanaLongitude);
        Angle planetSayanaLongitude2 = GetPlanetSayanaLongitude(time, PlanetName.Moon);
        PlanetLongitude item2 = new PlanetLongitude(PlanetName.Moon, planetSayanaLongitude2);
        Angle planetSayanaLongitude3 = GetPlanetSayanaLongitude(time, PlanetName.Mars);
        PlanetLongitude item3 = new PlanetLongitude(PlanetName.Mars, planetSayanaLongitude3);
        Angle planetSayanaLongitude4 = GetPlanetSayanaLongitude(time, PlanetName.Mercury);
        PlanetLongitude item4 = new PlanetLongitude(PlanetName.Mercury, planetSayanaLongitude4);
        Angle planetSayanaLongitude5 = GetPlanetSayanaLongitude(time, PlanetName.Jupiter);
        PlanetLongitude item5 = new PlanetLongitude(PlanetName.Jupiter, planetSayanaLongitude5);
        Angle planetSayanaLongitude6 = GetPlanetSayanaLongitude(time, PlanetName.Venus);
        PlanetLongitude item6 = new PlanetLongitude(PlanetName.Venus, planetSayanaLongitude6);
        Angle planetSayanaLongitude7 = GetPlanetSayanaLongitude(time, PlanetName.Saturn);
        PlanetLongitude item7 = new PlanetLongitude(PlanetName.Saturn, planetSayanaLongitude7);
        Angle planetSayanaLongitude8 = GetPlanetSayanaLongitude(time, PlanetName.Rahu);
        PlanetLongitude item8 = new PlanetLongitude(PlanetName.Rahu, planetSayanaLongitude8);
        Angle planetSayanaLongitude9 = GetPlanetSayanaLongitude(time, PlanetName.Ketu);
        PlanetLongitude item9 = new PlanetLongitude(PlanetName.Ketu, planetSayanaLongitude9);
        return new List<PlanetLongitude> { item, item2, item3, item4, item5, item6, item7, item9, item8 };
    }

    public static int GetHousePlanetIsIn(Time time, PlanetName planetName)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planetName);
        List<House> houses = GetHouses(time);
        foreach (House item in houses)
        {
            if (item.IsLongitudeInHouseRange(planetNirayanaLongitude))
            {
                return item.GetHouseNumber();
            }
        }

        throw new Exception("Planet not in any house, error!");
    }

    public static PlanetName GetLordOfHouse(HouseName houseNumber, Time time)
    {
        ZodiacName houseSignName = GetHouseSignName((int)houseNumber, time);
        return GetLordOfZodiacSign(houseSignName);
    }

    public static List<PlanetName> GetLordOfHouseList(List<HouseName> houseList, Time time)
    {
        List<PlanetName> list = new List<PlanetName>();
        foreach (HouseName house in houseList)
        {
            PlanetName lordOfHouse = GetLordOfHouse(house, time);
            list.Add(lordOfHouse);
        }

        return list;
    }

    public static bool IsHouseSignName(int house, ZodiacName sign, Time time)
    {
        return GetHouseSignName(house, time) == sign;
    }

    public static ZodiacName GetHouseSignName(int houseNumber, Time time)
    {
        List<House> houses = GetHouses(time);
        Angle middleLongitude = houses.Find((House house) => house.GetHouseNumber() == houseNumber).GetMiddleLongitude();
        Angle longitude = Angle.FromDegrees(Math.Round(middleLongitude.TotalDegrees, 4));
        ZodiacName signName = GetZodiacSignAtLongitude(longitude).GetSignName();
        ZodiacName signName2 = GetZodiacSignAtLongitude(middleLongitude).GetSignName();
        return signName;
    }

    public static ZodiacName GetNavamsaSignNameFromLongitude(Angle longitude)
    {
        ZodiacSign zodiacSignAtLongitude = GetZodiacSignAtLongitude(longitude);
        ZodiacName inputSign;
        switch (zodiacSignAtLongitude.GetSignName())
        {
            case ZodiacName.Aries:
            case ZodiacName.Leo:
            case ZodiacName.Sagittarius:
                inputSign = ZodiacName.Aries;
                break;
            case ZodiacName.Taurus:
            case ZodiacName.Virgo:
            case ZodiacName.Capricorn:
                inputSign = ZodiacName.Capricorn;
                break;
            case ZodiacName.Gemini:
            case ZodiacName.Libra:
            case ZodiacName.Aquarius:
                inputSign = ZodiacName.Libra;
                break;
            case ZodiacName.Cancer:
            case ZodiacName.Scorpio:
            case ZodiacName.Pisces:
                inputSign = ZodiacName.Cancer;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Angle degreesInSign = zodiacSignAtLongitude.GetDegreesInSign();
        double a = degreesInSign.TotalDegrees / 3.333333333;
        int countToNextSign = (int)Math.Ceiling(a);
        return GetSignCountedFromInputSign(inputSign, countToNextSign);
    }

    public static ZodiacName GetSignCountedFromInputSign(ZodiacName inputSign, int countToNextSign)
    {
        ZodiacName zodiacName = inputSign;
        for (int i = 1; i < countToNextSign; i++)
        {
            zodiacName = GetNextZodiacSign(zodiacName);
        }

        return zodiacName;
    }

    public static int GetHouseCountedFromInputHouse(int inputHouseNumber, int countToNextHouse)
    {
        int num = inputHouseNumber;
        for (int i = 1; i < countToNextHouse; i++)
        {
            num = GetNextHouseNumber(num);
        }

        return num;
    }

    public static ZodiacSign GetPlanetRasiSign(PlanetName planetName, Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planetName);
        return GetZodiacSignAtLongitude(planetNirayanaLongitude);
    }

    public static bool IsPlanetInSign(PlanetName planetName, ZodiacName signInput, Time time)
    {
        ZodiacName signName = GetPlanetRasiSign(planetName, time).GetSignName();
        return signName == signInput;
    }

    public static ZodiacName GetPlanetNavamsaSign(PlanetName planetName, Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planetName);
        return GetNavamsaSignNameFromLongitude(planetNirayanaLongitude);
    }

    public static List<ZodiacName> GetSignsPlanetIsAspecting(PlanetName planetName, Time time)
    {
        List<ZodiacName> list = new List<ZodiacName>();
        ZodiacName signName = GetPlanetRasiSign(planetName, time).GetSignName();
        if (planetName == PlanetName.Saturn)
        {
            ZodiacName signCountedFromInputSign = GetSignCountedFromInputSign(signName, 3);
            ZodiacName signCountedFromInputSign2 = GetSignCountedFromInputSign(signName, 10);
            list.Add(signCountedFromInputSign);
            list.Add(signCountedFromInputSign2);
        }

        if (planetName == PlanetName.Jupiter)
        {
            ZodiacName signCountedFromInputSign3 = GetSignCountedFromInputSign(signName, 5);
            ZodiacName signCountedFromInputSign4 = GetSignCountedFromInputSign(signName, 9);
            list.Add(signCountedFromInputSign3);
            list.Add(signCountedFromInputSign4);
        }

        if (planetName == PlanetName.Mars)
        {
            ZodiacName signCountedFromInputSign5 = GetSignCountedFromInputSign(signName, 4);
            ZodiacName signCountedFromInputSign6 = GetSignCountedFromInputSign(signName, 8);
            list.Add(signCountedFromInputSign5);
            list.Add(signCountedFromInputSign6);
        }

        ZodiacName signCountedFromInputSign7 = GetSignCountedFromInputSign(signName, 7);
        list.Add(signCountedFromInputSign7);
        return list;
    }

    public static ZodiacName GetHouseNavamsaSign(HouseName house, Time time)
    {
        List<House> houses = GetHouses(time);
        Angle middleLongitude = houses.Find((House hs) => hs.GetHouseNumber() == (int)house).GetMiddleLongitude();
        return GetNavamsaSignNameFromLongitude(middleLongitude);
    }

    public static ZodiacName GetPlanetThrimsamsaSign(PlanetName planetName, Time time)
    {
        ZodiacSign planetRasiSign = GetPlanetRasiSign(planetName, time);
        ZodiacName signName = planetRasiSign.GetSignName();
        double totalDegrees = planetRasiSign.GetDegreesInSign().TotalDegrees;
        double a = totalDegrees % 30.0 / 1.0;
        int num = (int)Math.Ceiling(a);
        if (IsOddSign(signName))
        {
            if (num >= 0 && num <= 5)
            {
                return ZodiacName.Scorpio;
            }

            if (num >= 6 && num <= 10)
            {
                return ZodiacName.Capricorn;
            }

            if (num >= 11 && num <= 18)
            {
                return ZodiacName.Sagittarius;
            }

            if (num >= 19 && num <= 25)
            {
                return ZodiacName.Gemini;
            }

            if (num >= 26 && num <= 30)
            {
                return ZodiacName.Taurus;
            }
        }

        if (IsEvenSign(signName))
        {
            if (num >= 0 && num <= 5)
            {
                return ZodiacName.Taurus;
            }

            if (num >= 6 && num <= 12)
            {
                return ZodiacName.Gemini;
            }

            if (num >= 13 && num <= 20)
            {
                return ZodiacName.Sagittarius;
            }

            if (num >= 21 && num <= 25)
            {
                return ZodiacName.Capricorn;
            }

            if (num >= 26 && num <= 30)
            {
                return ZodiacName.Scorpio;
            }
        }

        throw new Exception("Thrimsamsa not found, error!");
    }

    public static ZodiacName GetPlanetDwadasamsaSign(PlanetName planetName, Time time)
    {
        ZodiacSign planetRasiSign = GetPlanetRasiSign(planetName, time);
        ZodiacName signName = planetRasiSign.GetSignName();
        double totalDegrees = planetRasiSign.GetDegreesInSign().TotalDegrees;
        double a = totalDegrees % 30.0 / 2.5;
        int countToNextSign = (int)Math.Ceiling(a);
        return GetSignCountedFromInputSign(signName, countToNextSign);
    }

    public static ZodiacName GetPlanetSaptamsaSign(PlanetName planetName, Time time)
    {
        ZodiacSign planetRasiSign = GetPlanetRasiSign(planetName, time);
        ZodiacName signName = planetRasiSign.GetSignName();
        double totalDegrees = planetRasiSign.GetDegreesInSign().TotalDegrees;
        double a = totalDegrees % 30.0 / 4.2857142857142856;
        int num = (int)Math.Ceiling(a);
        if (IsOddSign(signName))
        {
            return GetSignCountedFromInputSign(signName, num);
        }

        if (IsEvenSign(signName))
        {
            int countToNextSign = num + 6;
            return GetSignCountedFromInputSign(signName, countToNextSign);
        }

        throw new Exception("Saptamsa not found, error!");
    }

    public static ZodiacName GetPlanetDrekkanaSign(PlanetName planetName, Time time)
    {
        ZodiacSign planetRasiSign = GetPlanetRasiSign(planetName, time);
        ZodiacName signName = planetRasiSign.GetSignName();
        double totalDegrees = planetRasiSign.GetDegreesInSign().TotalDegrees;
        if (totalDegrees >= 0.0 && totalDegrees <= 10.0)
        {
            return signName;
        }

        if (totalDegrees > 10.0 && totalDegrees <= 20.0)
        {
            return GetSignCountedFromInputSign(signName, 5);
        }

        if (totalDegrees > 20.0 && totalDegrees <= 30.0)
        {
            return GetSignCountedFromInputSign(signName, 9);
        }

        throw new Exception("Planet drekkana not found, error!");
    }

    public static bool IsPlanetInMoolatrikona(PlanetName planetName, Time time)
    {
        ZodiacSign planetRasiSign = GetPlanetRasiSign(planetName, time);
        if (planetName == PlanetName.Sun && planetRasiSign.GetSignName() == ZodiacName.Leo)
        {
            double totalDegrees = planetRasiSign.GetDegreesInSign().TotalDegrees;
            if (totalDegrees >= 0.0 && totalDegrees <= 20.0)
            {
                return true;
            }
        }

        if (planetName == PlanetName.Moon && planetRasiSign.GetSignName() == ZodiacName.Taurus)
        {
            double totalDegrees2 = planetRasiSign.GetDegreesInSign().TotalDegrees;
            if (totalDegrees2 >= 4.0 && totalDegrees2 <= 30.0)
            {
                return true;
            }
        }

        if (planetName == PlanetName.Mercury && planetRasiSign.GetSignName() == ZodiacName.Virgo)
        {
            double totalDegrees3 = planetRasiSign.GetDegreesInSign().TotalDegrees;
            if (totalDegrees3 >= 16.0 && totalDegrees3 <= 20.0)
            {
                return true;
            }
        }

        if (planetName == PlanetName.Jupiter && planetRasiSign.GetSignName() == ZodiacName.Sagittarius)
        {
            double totalDegrees4 = planetRasiSign.GetDegreesInSign().TotalDegrees;
            if (totalDegrees4 >= 0.0 && totalDegrees4 <= 13.0)
            {
                return true;
            }
        }

        if (planetName == PlanetName.Mars && planetRasiSign.GetSignName() == ZodiacName.Aries)
        {
            double totalDegrees5 = planetRasiSign.GetDegreesInSign().TotalDegrees;
            if (totalDegrees5 >= 0.0 && totalDegrees5 <= 18.0)
            {
                return true;
            }
        }

        if (planetName == PlanetName.Venus && planetRasiSign.GetSignName() == ZodiacName.Libra)
        {
            double totalDegrees6 = planetRasiSign.GetDegreesInSign().TotalDegrees;
            if (totalDegrees6 >= 0.0 && totalDegrees6 <= 10.0)
            {
                return true;
            }
        }

        if (planetName == PlanetName.Saturn && planetRasiSign.GetSignName() == ZodiacName.Aquarius)
        {
            double totalDegrees7 = planetRasiSign.GetDegreesInSign().TotalDegrees;
            if (totalDegrees7 >= 0.0 && totalDegrees7 <= 20.0)
            {
                return true;
            }
        }

        return false;
    }

    public static ZodiacName GetPlanetHoraSign(PlanetName planetName, Time time)
    {
        ZodiacSign planetRasiSign = GetPlanetRasiSign(planetName, time);
        ZodiacName signName = planetRasiSign.GetSignName();
        double totalDegrees = planetRasiSign.GetDegreesInSign().TotalDegrees;
        bool flag = false;
        bool flag2 = false;
        if (totalDegrees >= 0.0 && totalDegrees <= 15.0)
        {
            flag = true;
        }

        if (totalDegrees > 15.0 && totalDegrees <= 30.0)
        {
            flag2 = true;
        }

        if (IsOddSign(signName))
        {
            if (flag && !flag2)
            {
                return ZodiacName.Leo;
            }

            if (!flag && flag2)
            {
                return ZodiacName.Cancer;
            }
        }

        if (IsEvenSign(signName))
        {
            if (flag && !flag2)
            {
                return ZodiacName.Cancer;
            }

            if (!flag && flag2)
            {
                return ZodiacName.Leo;
            }
        }

        throw new Exception("Planet hora not found, error!");
    }

    public static PlanetToSignRelationship GetPlanetRelationshipWithSign(PlanetName planetName, ZodiacName zodiacSignName, Time time)
    {
        PlanetName lordOfZodiacSign = GetLordOfZodiacSign(zodiacSignName);
        if (planetName == lordOfZodiacSign)
        {
            return PlanetToSignRelationship.OwnVarga;
        }

        return GetPlanetCombinedRelationshipWithPlanet(planetName, lordOfZodiacSign, time) switch
        {
            PlanetToPlanetRelationship.BestFriend => PlanetToSignRelationship.BestFriendVarga,
            PlanetToPlanetRelationship.Friend => PlanetToSignRelationship.FriendVarga,
            PlanetToPlanetRelationship.BitterEnemy => PlanetToSignRelationship.BitterEnemyVarga,
            PlanetToPlanetRelationship.Enemy => PlanetToSignRelationship.EnemyVarga,
            PlanetToPlanetRelationship.Neutral => PlanetToSignRelationship.NeutralVarga,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static PlanetToPlanetRelationship GetPlanetCombinedRelationshipWithPlanet(PlanetName mainPlanet, PlanetName secondaryPlanet, Time time)
    {
        if (mainPlanet == secondaryPlanet)
        {
            return PlanetToPlanetRelationship.SamePlanet;
        }

        PlanetToPlanetRelationship planetPermanentRelationshipWithPlanet = GetPlanetPermanentRelationshipWithPlanet(mainPlanet, secondaryPlanet);
        PlanetToPlanetRelationship planetTemporaryRelationshipWithPlanet = GetPlanetTemporaryRelationshipWithPlanet(mainPlanet, secondaryPlanet, time);
        if (planetTemporaryRelationshipWithPlanet == PlanetToPlanetRelationship.Friend && planetPermanentRelationshipWithPlanet == PlanetToPlanetRelationship.Friend)
        {
            return PlanetToPlanetRelationship.BestFriend;
        }

        if (planetTemporaryRelationshipWithPlanet == PlanetToPlanetRelationship.Friend && planetPermanentRelationshipWithPlanet == PlanetToPlanetRelationship.Enemy)
        {
            return PlanetToPlanetRelationship.Neutral;
        }

        if (planetTemporaryRelationshipWithPlanet == PlanetToPlanetRelationship.Friend && planetPermanentRelationshipWithPlanet == PlanetToPlanetRelationship.Neutral)
        {
            return PlanetToPlanetRelationship.Friend;
        }

        if (planetTemporaryRelationshipWithPlanet == PlanetToPlanetRelationship.Enemy && planetPermanentRelationshipWithPlanet == PlanetToPlanetRelationship.Enemy)
        {
            return PlanetToPlanetRelationship.BitterEnemy;
        }

        if (planetTemporaryRelationshipWithPlanet == PlanetToPlanetRelationship.Enemy && planetPermanentRelationshipWithPlanet == PlanetToPlanetRelationship.Friend)
        {
            return PlanetToPlanetRelationship.Neutral;
        }

        if (planetTemporaryRelationshipWithPlanet == PlanetToPlanetRelationship.Enemy && planetPermanentRelationshipWithPlanet == PlanetToPlanetRelationship.Neutral)
        {
            return PlanetToPlanetRelationship.Enemy;
        }

        throw new Exception("Combined planet relationship not found, error!");
    }

    public static PlanetToSignRelationship GetPlanetRelationshipWithHouse(HouseName house, PlanetName planet, Time time)
    {
        ZodiacName houseSignName = GetHouseSignName((int)house, time);
        return GetPlanetRelationshipWithSign(planet, houseSignName, time);
    }

    public static PlanetToPlanetRelationship GetPlanetTemporaryRelationshipWithPlanet(PlanetName mainPlanet, PlanetName secondaryPlanet, Time time)
    {
        if (mainPlanet == secondaryPlanet)
        {
            return PlanetToPlanetRelationship.SamePlanet;
        }

        List<PlanetName> planetTemporaryFriendList = GetPlanetTemporaryFriendList(mainPlanet, time);
        if (planetTemporaryFriendList.Contains(secondaryPlanet))
        {
            return PlanetToPlanetRelationship.Friend;
        }

        return PlanetToPlanetRelationship.Enemy;
    }

    public static List<PlanetName> GetPlanetInSign(ZodiacName signName, Time time)
    {
        ZodiacName signName2 = GetPlanetRasiSign(PlanetName.Sun, time).GetSignName();
        ZodiacName signName3 = GetPlanetRasiSign(PlanetName.Moon, time).GetSignName();
        ZodiacName signName4 = GetPlanetRasiSign(PlanetName.Mars, time).GetSignName();
        ZodiacName signName5 = GetPlanetRasiSign(PlanetName.Mercury, time).GetSignName();
        ZodiacName signName6 = GetPlanetRasiSign(PlanetName.Jupiter, time).GetSignName();
        ZodiacName signName7 = GetPlanetRasiSign(PlanetName.Venus, time).GetSignName();
        ZodiacName signName8 = GetPlanetRasiSign(PlanetName.Saturn, time).GetSignName();
        ZodiacName signName9 = GetPlanetRasiSign(PlanetName.Rahu, time).GetSignName();
        ZodiacName signName10 = GetPlanetRasiSign(PlanetName.Ketu, time).GetSignName();
        List<PlanetName> list = new List<PlanetName>();
        if (signName2 == signName)
        {
            list.Add(PlanetName.Sun);
        }

        if (signName3 == signName)
        {
            list.Add(PlanetName.Moon);
        }

        if (signName4 == signName)
        {
            list.Add(PlanetName.Mars);
        }

        if (signName5 == signName)
        {
            list.Add(PlanetName.Mercury);
        }

        if (signName6 == signName)
        {
            list.Add(PlanetName.Jupiter);
        }

        if (signName7 == signName)
        {
            list.Add(PlanetName.Venus);
        }

        if (signName8 == signName)
        {
            list.Add(PlanetName.Saturn);
        }

        if (signName9 == signName)
        {
            list.Add(PlanetName.Rahu);
        }

        if (signName10 == signName)
        {
            list.Add(PlanetName.Ketu);
        }

        return list;
    }

    public static List<PlanetName> GetPlanetTemporaryFriendList(PlanetName planetName, Time time)
    {
        ZodiacName signName = GetPlanetRasiSign(planetName, time).GetSignName();
        ZodiacName signCountedFromInputSign = GetSignCountedFromInputSign(signName, 2);
        ZodiacName signCountedFromInputSign2 = GetSignCountedFromInputSign(signName, 3);
        ZodiacName signCountedFromInputSign3 = GetSignCountedFromInputSign(signName, 4);
        ZodiacName signCountedFromInputSign4 = GetSignCountedFromInputSign(signName, 10);
        ZodiacName signCountedFromInputSign5 = GetSignCountedFromInputSign(signName, 11);
        ZodiacName signCountedFromInputSign6 = GetSignCountedFromInputSign(signName, 12);
        List<ZodiacName> list = new List<ZodiacName> { signCountedFromInputSign, signCountedFromInputSign2,
            signCountedFromInputSign3, signCountedFromInputSign4, signCountedFromInputSign5, signCountedFromInputSign6 };
        List<PlanetName> list2 = new List<PlanetName>();
        foreach (ZodiacName item in list)
        {
            List<PlanetName> planetInSign = GetPlanetInSign(item, time);
            list2.AddRange(planetInSign);
        }

        list2.Remove(PlanetName.Rahu);
        list2.Remove(PlanetName.Ketu);
        return list2;
    }

    public static double GetGreenwichApparentInJulianDays(Time time)
    {
        double greenwichLmtInJulianDays = GetGreenwichLmtInJulianDays(time);
        double longitude = time.GetGeoLocation().GetLongitude();
        string serr = "";
        using SwissEph swissEph = new SwissEph();
        swissEph.swe_lmt_to_lat(greenwichLmtInJulianDays, longitude, out var tjd_lat, ref serr);
        return tjd_lat;
    }

    public static DateTime GetLocalApparentTime(Time time)
    {
        double tjd_lmt = ConvertLmtToJulian(time);
        double longitude = time.GetGeoLocation().GetLongitude();
        string serr = null;
        SwissEph swissEph = new SwissEph();
        swissEph.swe_lmt_to_lat(tjd_lmt, longitude, out var tjd_lat, ref serr);
        return ConvertJulianTimeToNormalTime(tjd_lat);
    }

    public static DateTimeOffset GetLocalMeanTime(Time time)
    {
        return time.GetLmtDateTimeOffset();
    }

    public static House GetHouse(HouseName houseNumber, Time time)
    {
        List<House> houses = GetHouses(time);
        return houses.Find((House h) => h.GetHouseNumber() == (int)houseNumber);
    }

    public static PanchakaName GetPanchaka(Time time)
    {
        int lunarDateNumber = GetLunarDay(time).GetLunarDateNumber();
        int constellationNumber = GetMoonConstellation(time).GetConstellationNumber();
        int dayOfWeek = (int)GetDayOfWeek(time);
        int houseSignName = (int)GetHouseSignName(1, time);
        double num = lunarDateNumber + constellationNumber + dayOfWeek + houseSignName;
        double num2 = num % 9.0;
        double num3 = num2;
        double num4 = num3;
        if (num4 != 1.0)
        {
            if (num4 != 2.0)
            {
                if (num4 != 4.0)
                {
                    if (num4 != 6.0)
                    {
                        if (num4 != 8.0)
                        {
                            if (num4 == 3.0 || num4 == 5.0 || num4 == 7.0 || num4 == 0.0)
                            {
                                return PanchakaName.Shubha;
                            }

                            throw new Exception("Panchaka not found, error!");
                        }

                        return PanchakaName.Roga;
                    }

                    return PanchakaName.Chora;
                }

                return PanchakaName.Raja;
            }

            return PanchakaName.Agni;
        }

        return PanchakaName.Mrityu;
    }

    public static PlanetName GetLordOfWeekday(Time time)
    {
        return GetDayOfWeek(time) switch
        {
            DayOfWeek.Sunday => PlanetName.Sun,
            DayOfWeek.Monday => PlanetName.Moon,
            DayOfWeek.Tuesday => PlanetName.Mars,
            DayOfWeek.Wednesday => PlanetName.Mercury,
            DayOfWeek.Thursday => PlanetName.Jupiter,
            DayOfWeek.Friday => PlanetName.Venus,
            DayOfWeek.Saturday => PlanetName.Saturn,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static PlanetName GetLordOfWeekday(DayOfWeek weekday)
    {
        return weekday switch
        {
            DayOfWeek.Sunday => PlanetName.Sun,
            DayOfWeek.Monday => PlanetName.Moon,
            DayOfWeek.Tuesday => PlanetName.Mars,
            DayOfWeek.Wednesday => PlanetName.Mercury,
            DayOfWeek.Thursday => PlanetName.Jupiter,
            DayOfWeek.Friday => PlanetName.Venus,
            DayOfWeek.Saturday => PlanetName.Saturn,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static DateTimeOffset LmtToStd(DateTimeOffset lmtDateTime, TimeSpan stdOffset)
    {
        return lmtDateTime.ToOffset(stdOffset);
    }

    public static int GetHoraAtBirth(Time time)
    {
        DateTimeOffset lmtDateTimeOffset = time.GetLmtDateTimeOffset();
        DateTimeOffset lmtDateTimeOffset2 = GetSunriseTime(time).GetLmtDateTimeOffset();
        TimeSpan timeSpan;
        if (lmtDateTimeOffset >= lmtDateTimeOffset2)
        {
            timeSpan = lmtDateTimeOffset.Subtract(lmtDateTimeOffset2);
        }
        else
        {
            Time time2 = new Time(time.GetLmtDateTimeOffset().DateTime.AddDays(-1.0), time.GetStdDateTimeOffset().Offset, time.GetGeoLocation());
            lmtDateTimeOffset2 = GetSunriseTime(time2).GetLmtDateTimeOffset();
            timeSpan = lmtDateTimeOffset.Subtract(lmtDateTimeOffset2);
        }

        double num = Math.Ceiling(timeSpan.TotalHours);
        if (num == 0.0)
        {
            num = 1.0;
        }

        return (int)num;
    }

    public static Time GetSunriseTime(Time time)
    {
        int rsmi = 769;
        int ipl = 0;
        double[] geopos = new double[3]
        {
                time.GetGeoLocation().GetLongitude(),
                time.GetGeoLocation().GetLatitude(),
                0.0
        };
        double tret = 0.0;
        string serr = "";
        DateTimeOffset lmtDateTimeOffset = time.GetLmtDateTimeOffset();
        DateTime lmtDateTime = new DateTime(lmtDateTimeOffset.Year, lmtDateTimeOffset.Month, lmtDateTimeOffset.Day, 0, 0, 0);
        Time time2 = new Time(lmtDateTime, time.GetStdDateTimeOffset().Offset, time.GetGeoLocation());
        double greenwichLmtInJulianDays = GetGreenwichLmtInJulianDays(time2);
        using SwissEph swissEph = new SwissEph();
        int num = swissEph.swe_rise_trans(greenwichLmtInJulianDays, ipl, "", 65794, rsmi, geopos, 0.0, 0.0, ref tret, ref serr);
        DateTimeOffset stdDateTime = GetGreenwichTimeFromJulianDays(tret).ToOffset(time.GetStdDateTimeOffset().Offset);
        return new Time(stdDateTime, time.GetGeoLocation());
    }

    public static Time GetSunsetTime(Time time)
    {
        int rsmi = 770;
        int ipl = 0;
        double[] geopos = new double[3]
        {
                time.GetGeoLocation().GetLongitude(),
                time.GetGeoLocation().GetLatitude(),
                0.0
        };
        double tret = 0.0;
        string serr = "";
        DateTimeOffset lmtDateTimeOffset = time.GetLmtDateTimeOffset();
        DateTime lmtDateTime = new DateTime(lmtDateTimeOffset.Year, lmtDateTimeOffset.Month, lmtDateTimeOffset.Day, 0, 0, 0);
        Time time2 = new Time(lmtDateTime, time.GetStdDateTimeOffset().Offset, time.GetGeoLocation());
        double greenwichLmtInJulianDays = GetGreenwichLmtInJulianDays(time2);
        using SwissEph swissEph = new SwissEph();
        int num = swissEph.swe_rise_trans(greenwichLmtInJulianDays, ipl, "", 65794, rsmi, geopos, 0.0, 0.0, ref tret, ref serr);
        DateTimeOffset stdDateTime = GetGreenwichTimeFromJulianDays(tret).ToOffset(time.GetStdDateTimeOffset().Offset);
        return new Time(stdDateTime, time.GetGeoLocation());
    }

    public static DateTime GetNoonTime(Time time)
    {
        DateTime localApparentTime = GetLocalApparentTime(time);
        return new DateTime(localApparentTime.Year, localApparentTime.Month, localApparentTime.Day, 12, 0, 0);
    }

    public static bool IsPlanetInGoodAspectToPlanet(PlanetName receivingAspect, PlanetName transmitingAspect, Time time)
    {
        bool flag = IsPlanetAspectedByPlanet(receivingAspect, transmitingAspect, time);
        if (!flag)
        {
            return false;
        }

        PlanetToPlanetRelationship planetCombinedRelationshipWithPlanet = GetPlanetCombinedRelationshipWithPlanet(receivingAspect, transmitingAspect, time);
        bool flag2 = planetCombinedRelationshipWithPlanet == PlanetToPlanetRelationship.BestFriend || planetCombinedRelationshipWithPlanet == PlanetToPlanetRelationship.Friend;
        return flag && flag2;
    }

    public static bool IsPlanetInGoodAspectToHouse(HouseName receivingAspect, PlanetName transmitingAspect, Time time)
    {
        bool flag = IsHouseAspectedByPlanet(receivingAspect, transmitingAspect, time);
        if (!flag)
        {
            return false;
        }

        PlanetToSignRelationship planetRelationshipWithHouse = GetPlanetRelationshipWithHouse(receivingAspect, transmitingAspect, time);
        bool flag2 = planetRelationshipWithHouse == PlanetToSignRelationship.OwnVarga
            || planetRelationshipWithHouse == PlanetToSignRelationship.FriendVarga
            || planetRelationshipWithHouse == PlanetToSignRelationship.BestFriendVarga;
        return flag && flag2;
    }

    public static double GetPlanetSthanaBalaNeutralPoint(PlanetName planet)
    {
        int num = 0;
        int num2 = 0;
        if (planet == PlanetName.Saturn)
        {
            num = 297;
            num2 = 59;
        }

        if (planet == PlanetName.Mars)
        {
            num = 362;
            num2 = 60;
        }

        if (planet == PlanetName.Jupiter)
        {
            num = 296;
            num2 = 77;
        }

        if (planet == PlanetName.Mercury)
        {
            num = 295;
            num2 = 47;
        }

        if (planet == PlanetName.Venus)
        {
            num = 284;
            num2 = 60;
        }

        if (planet == PlanetName.Sun)
        {
            num = 327;
            num2 = 52;
        }

        if (planet == PlanetName.Moon)
        {
            num = 311;
            num2 = 54;
        }

        int num3 = (num - num2) / 2 + num2;
        if (num3 <= 0)
        {
            throw new Exception("Planet does not have neutral point!");
        }

        return num3;
    }

    public static double GetPlanetShadvargaBalaNeutralPoint(PlanetName planet)
    {
        int num = 0;
        int num2 = 0;
        if (planet == PlanetName.Saturn)
        {
            num = 150;
            num2 = 11;
        }

        if (planet == PlanetName.Mars)
        {
            num = 188;
            num2 = 21;
        }

        if (planet == PlanetName.Jupiter)
        {
            num = 172;
            num2 = 17;
        }

        if (planet == PlanetName.Mercury)
        {
            num = 150;
            num2 = 17;
        }

        if (planet == PlanetName.Venus)
        {
            num = 158;
            num2 = 15;
        }

        if (planet == PlanetName.Sun)
        {
            num = 180;
            num2 = 17;
        }

        if (planet == PlanetName.Moon)
        {
            num = 165;
            num2 = 26;
        }

        int num3 = num - num2;
        int num4 = num3 / 2 + num2;
        if (num4 <= 0)
        {
            throw new Exception("Planet does not have neutral point!");
        }

        return num4;
    }

    public static bool IsPlanetInKendra(PlanetName planet, Time time)
    {
        int housePlanetIsIn = GetHousePlanetIsIn(time, planet);
        return housePlanetIsIn == 4 || housePlanetIsIn == 7 || housePlanetIsIn == 10;
    }

    public static bool IsHouseLordInHouse(HouseName lordHouse, HouseName occupiedHouse, Time time)
    {
        PlanetName lordOfHouse = GetLordOfHouse(lordHouse, time);
        int housePlanetIsIn = GetHousePlanetIsIn(time, lordOfHouse);
        return housePlanetIsIn == (int)occupiedHouse;
    }

    public static bool IsPlanetConjunctWithMaleficPlanets(PlanetName planetName, Time time)
    {
        List<PlanetName> planetsInConjuction = GetPlanetsInConjuction(time, planetName);
        List<PlanetName> evilPlanets = GetMaleficPlanetList(time);
        return planetsInConjuction.FindAll((PlanetName planet) => evilPlanets.Contains(planet)).Any();
    }

    public static bool IsMaleficPlanetInHouse(int houseNumber, Time time)
    {
        List<PlanetName> planetsInHouse = GetPlanetsInHouse(houseNumber, time);
        List<PlanetName> evilPlanets = GetMaleficPlanetList(time);
        return planetsInHouse.FindAll((PlanetName planet) => evilPlanets.Contains(planet)).Any();
    }

    public static bool IsBeneficPlanetInHouse(int houseNumber, Time time)
    {
        List<PlanetName> planetsInHouse = GetPlanetsInHouse(houseNumber, time);
        List<PlanetName> goodPlanets = GetBeneficPlanetList(time);
        return planetsInHouse.FindAll((PlanetName planet) => goodPlanets.Contains(planet)).Any();
    }

    public static bool IsMaleficPlanetInSign(ZodiacName sign, Time time)
    {
        List<PlanetName> planetInSign = GetPlanetInSign(sign, time);
        List<PlanetName> evilPlanets = GetMaleficPlanetList(time);
        return planetInSign.FindAll((PlanetName planet) => evilPlanets.Contains(planet)).Any();
    }

    public static List<PlanetName> GetMaleficPlanetListInSign(ZodiacName sign, Time time)
    {
        List<PlanetName> planetInSign = GetPlanetInSign(sign, time);
        List<PlanetName> evilPlanets = GetMaleficPlanetList(time);
        return planetInSign.FindAll((PlanetName planet) => evilPlanets.Contains(planet));
    }

    public static bool IsBeneficPlanetInSign(ZodiacName sign, Time time)
    {
        List<PlanetName> planetInSign = GetPlanetInSign(sign, time);
        List<PlanetName> goodPlanets = GetBeneficPlanetList(time);
        return planetInSign.FindAll((PlanetName planet) => goodPlanets.Contains(planet)).Any();
    }

    public static List<PlanetName> GetBeneficPlanetListInSign(ZodiacName sign, Time time)
    {
        List<PlanetName> planetInSign = GetPlanetInSign(sign, time);
        List<PlanetName> goodPlanets = GetBeneficPlanetList(time);
        return planetInSign.FindAll((PlanetName planet) => goodPlanets.Contains(planet));
    }

    public static bool IsMaleficPlanetAspectHouse(HouseName house, Time time)
    {
        List<PlanetName> maleficPlanetList = GetMaleficPlanetList(time);
        return maleficPlanetList.FindAll((PlanetName evilPlanet) => IsHouseAspectedByPlanet(house, evilPlanet, time)).Any();
    }

    public static bool IsBeneficPlanetAspectHouse(HouseName house, Time time)
    {
        List<PlanetName> beneficPlanetList = GetBeneficPlanetList(time);
        return beneficPlanetList.FindAll((PlanetName goodPlanet) => IsHouseAspectedByPlanet(house, goodPlanet, time)).Any();
    }

    public static bool IsPlanetAspectedByMaleficPlanets(PlanetName lord, Time time)
    {
        List<PlanetName> maleficPlanetList = GetMaleficPlanetList(time);
        return maleficPlanetList.FindAll((PlanetName evilPlanet) => IsPlanetAspectedByPlanet(lord, evilPlanet, time)).Any();
    }

    public static ZodiacName GetArudhaLagnaSign(Time time)
    {
        ZodiacName houseSignName = GetHouseSignName(1, time);
        PlanetName lordOfHouse = GetLordOfHouse(HouseName.House1, time);
        ZodiacName signName = GetPlanetRasiSign(lordOfHouse, time).GetSignName();
        int countToNextSign = CountFromSignToSign(houseSignName, signName);
        return GetSignCountedFromInputSign(signName, countToNextSign);
    }

    public static int CountFromSignToSign(ZodiacName startSign, ZodiacName endSign)
    {
        int result = 0;
        if (startSign > endSign)
        {
            int num = (int)(12 - startSign + 1);
            result = (int)(endSign + num);
        }
        else if (startSign == endSign)
        {
            result = 1;
        }
        else if (startSign < endSign)
        {
            result = endSign - startSign + 1;
        }

        return result;
    }

    public static int CountFromConstellationToConstellation(PlanetConstellation start, PlanetConstellation end)
    {
        int constellationNumber = end.GetConstellationNumber();
        int constellationNumber2 = start.GetConstellationNumber();
        int result = 0;
        if (constellationNumber2 > constellationNumber)
        {
            int num = 27 - constellationNumber2 + 1;
            result = constellationNumber + num;
        }
        else if (constellationNumber2 == constellationNumber)
        {
            result = 1;
        }
        else if (constellationNumber2 < constellationNumber)
        {
            result = constellationNumber - constellationNumber2 + 1;
        }

        return result;
    }

    public static bool IsPlanetInHouse(Time time, PlanetName planet, int houseNumber)
    {
        return GetHousePlanetIsIn(time, planet) == houseNumber;
    }

    public static bool IsPlanetDebilitated(PlanetName planet, Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planet);
        ZodiacSign zodiacSignAtLongitude = GetZodiacSignAtLongitude(planetNirayanaLongitude);
        ZodiacSign planetDebilitationPoint = GetPlanetDebilitationPoint(planet);
        bool flag = zodiacSignAtLongitude.GetSignName() == planetDebilitationPoint.GetSignName();
        bool flag2 = zodiacSignAtLongitude.GetDegreesInSign().Degrees == planetDebilitationPoint.GetDegreesInSign().Degrees;
        return flag && flag2;
    }

    public static bool IsPlanetExaltated(PlanetName planet, Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planet);
        ZodiacSign zodiacSignAtLongitude = GetZodiacSignAtLongitude(planetNirayanaLongitude);
        ZodiacSign planetExaltationPoint = GetPlanetExaltationPoint(planet);
        bool flag = zodiacSignAtLongitude.GetSignName() == planetExaltationPoint.GetSignName();
        bool flag2 = zodiacSignAtLongitude.GetDegreesInSign().Degrees == planetExaltationPoint.GetDegreesInSign().Degrees;
        return flag && flag2;
    }

    public static LunarMonth GetLunarMonth(Time time)
    {
        throw new NotImplementedException();
    }

    public static bool IsFullMoon(Time time)
    {
        int lunarDayNumber = GetLunarDay(time).GetLunarDayNumber();
        return lunarDayNumber == 15;
    }

    public static bool IsAquaticSign(ZodiacName moonSign)
    {
        if (moonSign == ZodiacName.Cancer || moonSign == ZodiacName.Scorpio || moonSign == ZodiacName.Pisces)
        {
            return true;
        }

        return false;
    }

    public static bool IsFireSign(ZodiacName moonSign)
    {
        if (moonSign == ZodiacName.Aries || moonSign == ZodiacName.Leo || moonSign == ZodiacName.Sagittarius)
        {
            return true;
        }

        return false;
    }

    public static bool IsEarthSign(ZodiacName moonSign)
    {
        if (moonSign == ZodiacName.Taurus || moonSign == ZodiacName.Virgo || moonSign == ZodiacName.Capricorn)
        {
            return true;
        }

        return false;
    }

    public static bool IsAirSign(ZodiacName moonSign)
    {
        if (moonSign == ZodiacName.Gemini || moonSign == ZodiacName.Libra || moonSign == ZodiacName.Aquarius)
        {
            return true;
        }

        return false;
    }

    public static string GetHouseDescription(int house)
    {
        return house switch
        {
            1 => "beginning of life, childhood, health, environment, personality, the physical body and character",
            2 => "family, face, right eye, food, wealth, literary gift, and manner and source of death, self-acquisition and optimism",
            3 => "brothers and sisters, intelligence, cousins and other immediate relations",
            4 => "peace of mind, home life, mother, conveyances, house property, landed and ancestral properties, education and neck and shoulders",
            5 => "children, grandfather, intelligence, emotions and fame",
            6 => "debts, diseases, enemies, miseries, sorrows, illness and disappointments",
            7 => "wife, husband, marriage, urinary organs, marital happiness, sexual diseases, business partner, diplomacy, talent, energies and general happiness",
            8 => "longevity, legacies and gifts and unearned wealth, cause of death, disgrace, degradation and details pertaining to death",
            9 => "father, righteousness, preceptor, grandchildren, intuition, religion, sympathy, fame, charities, leadership, journeys and communications with spirits",
            10 => "occupation, profession, temporal honours, foreign travels, self-respect, knowledge and dignity and means of livelihood",
            11 => "means of gains, elder brother and freedom from misery",
            12 => "losses, expenditure, waste, extravagance, sympathy, divine knowledge, Moksha and the state after death",
            _ => throw new Exception("House details not found!"),
        };
    }

    public static EventNature GetPlanetAntaramNature(Person person, PlanetName planet)
    {
        if (planet == PlanetName.Rahu || planet == PlanetName.Ketu)
        {
            return EventNature.Neutral;
        }

        EventNature eventNature = GetNatureFromLagna();
        if (eventNature == EventNature.Neutral)
        {
            int housePlanetIsIn = GetHousePlanetIsIn(person.BirthTime, planet);
            switch (GetPlanetRelationshipWithHouse((HouseName)housePlanetIsIn, planet, person.BirthTime))
            {
                case PlanetToSignRelationship.OwnVarga:
                case PlanetToSignRelationship.BestFriendVarga:
                case PlanetToSignRelationship.FriendVarga:
                case PlanetToSignRelationship.Moolatrikona:
                    return EventNature.Good;
                case PlanetToSignRelationship.NeutralVarga:
                    return EventNature.Neutral;
                case PlanetToSignRelationship.EnemyVarga:
                case PlanetToSignRelationship.BitterEnemyVarga:
                    return EventNature.Bad;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return eventNature;
        EventNature GetNatureFromLagna()
        {
            ZodiacName houseSignName = GetHouseSignName(1, person.BirthTime);
            dynamic planetData = GetPlanetData(houseSignName);
            List<PlanetName> list3 = planetData.Good;
            List<PlanetName> list4 = planetData.Bad;
            if (list3.Contains(planet))
            {
                return EventNature.Good;
            }

            if (list4.Contains(planet))
            {
                return EventNature.Bad;
            }

            return EventNature.Neutral;
        }

        static object GetPlanetData(ZodiacName lagna)
        {
            List<PlanetName> list = null;
            List<PlanetName> list2 = null;
            switch (lagna)
            {
                case ZodiacName.Aries:
                    list = new List<PlanetName>
                {
                    PlanetName.Jupiter,
                    PlanetName.Sun
                };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Saturn,
                    PlanetName.Mercury,
                    PlanetName.Venus
                };
                    break;
                case ZodiacName.Taurus:
                    list = new List<PlanetName> { PlanetName.Saturn };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Jupiter,
                    PlanetName.Venus,
                    PlanetName.Moon
                };
                    break;
                case ZodiacName.Gemini:
                    list = new List<PlanetName> { PlanetName.Venus };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Mars,
                    PlanetName.Jupiter,
                    PlanetName.Sun
                };
                    break;
                case ZodiacName.Cancer:
                    list = new List<PlanetName>
                {
                    PlanetName.Jupiter,
                    PlanetName.Mars
                };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Venus,
                    PlanetName.Mercury
                };
                    break;
                case ZodiacName.Leo:
                    list = new List<PlanetName> { PlanetName.Mars };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Saturn,
                    PlanetName.Venus,
                    PlanetName.Mercury
                };
                    break;
                case ZodiacName.Virgo:
                    list = new List<PlanetName> { PlanetName.Venus };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Mars,
                    PlanetName.Moon
                };
                    break;
                case ZodiacName.Libra:
                    list = new List<PlanetName>
                {
                    PlanetName.Saturn,
                    PlanetName.Mercury
                };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Jupiter,
                    PlanetName.Sun,
                    PlanetName.Mars
                };
                    break;
                case ZodiacName.Scorpio:
                    list = new List<PlanetName> { PlanetName.Jupiter };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Mercury,
                    PlanetName.Venus
                };
                    break;
                case ZodiacName.Sagittarius:
                    list = new List<PlanetName> { PlanetName.Mars };
                    list2 = new List<PlanetName> { PlanetName.Venus };
                    break;
                case ZodiacName.Capricorn:
                    list = new List<PlanetName> { PlanetName.Venus };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Mars,
                    PlanetName.Jupiter,
                    PlanetName.Moon
                };
                    break;
                case ZodiacName.Aquarius:
                    list = new List<PlanetName> { PlanetName.Venus };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Jupiter,
                    PlanetName.Moon
                };
                    break;
                case ZodiacName.Pisces:
                    list = new List<PlanetName>
                {
                    PlanetName.Moon,
                    PlanetName.Mars
                };
                    list2 = new List<PlanetName>
                {
                    PlanetName.Saturn,
                    PlanetName.Venus,
                    PlanetName.Sun,
                    PlanetName.Mercury
                };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new
            {
                Good = list,
                Bad = list2
            };
        }
    }

    public static string GetDasaInfoForAscendant(ZodiacName acesendatName)
    {
        return acesendatName switch
        {
            ZodiacName.Aries => "\r\n                        Aries - Saturn, Mercury and Venus are ill-disposed.\r\n                        Jupiter and the Sun are auspicious. The mere combination\r\n                        of Jupiler and Saturn produces no beneficial results. Jupiter\r\n                        is the Yogakaraka or the planet producing success. If Venus\r\n                        becomes a maraka, he will not kill the native but planets like\r\n                        Saturn will bring about death to the person.\r\n                        ",
            ZodiacName.Taurus => "\r\n                        Taurus - Saturn is the most auspicious and powerful\r\n                        planet. Jupiter, Venus and the Moon are evil planets. Saturn\r\n                        alone produces Rajayoga. The native will be killed in the\r\n                        periods and sub-periods of Jupiter, Venus and the Moon if\r\n                        they get death-inflicting powers.\r\n                        ",
            ZodiacName.Gemini => "\r\n                        Gemini - Mars, Jupiter and the Sun are evil. Venus alone\r\n                        is most beneficial and in conjunction with Saturn in good signs\r\n                        produces and excellent career of much fame. Combination\r\n                        of Saturn and Jupiter produces similar results as in Aries.\r\n                        Venus and Mercury, when well associated, cause Rajayoga.\r\n                        The Moon will not kill the person even though possessed of\r\n                        death-inflicting powers.\r\n                        ",
            ZodiacName.Cancer => "\r\n                        Cancer - Venus and Mercury are evil. Jupiter and Mars\r\n                        give beneficial results. Mars is the Rajayogakaraka\r\n                        (conferor of name and fame). The combination of Mars and Jupiter\r\n                        also causes Rajayoga (combination for political success). The\r\n                        Sun does not kill the person although possessed of maraka\r\n                        powers. Venus and other inauspicious planets kill the native.\r\n                        Mars in combination with the Moon or Jupiter in favourable\r\n                        houses especially the 1st, the 5th, the 9th and the 10th\r\n                        produces much reputation.\r\n                        ",
            ZodiacName.Leo => "\r\n                        Leo - Mars is the most auspicious and favourable planet.\r\n                        The combination of Venus and Jupiter does not cause Rajayoga\r\n                        but the conjunction of Jupiter and Mars in favourable\r\n                        houses produce Rajayoga. Saturn, Venus and Mercury are\r\n                        evil. Saturn does not kill the native when he has the maraka\r\n                        power but Mercury and other evil planets inflict death when\r\n                        they get maraka powers.\r\n                        ",
            ZodiacName.Virgo => "\r\n                        Virgo - Venus alone is the most powerful. Mercury and\r\n                        Venus when combined together cause Rajayoga. Mars and\r\n                        the Moon are evil. The Sun does not kill the native even if\r\n                        be becomes a maraka but Venus, the Moon and Jupiter will\r\n                        inflict death when they are possessed of death-infticting power.\r\n                        ",
            ZodiacName.Libra => "\r\n                        Libra - Saturn alone causes Rajayoga. Jupiter, the Sun\r\n                        and Mars are inauspicious. Mercury and Saturn produce good.\r\n                        The conjunction of the Moon and Mercury produces Rajayoga.\r\n                        Mars himself will not kill the person. Jupiter, Venus\r\n                        and Mars when possessed of maraka powers certainly kill the\r\n                        nalive.\r\n                        ",
            ZodiacName.Scorpio => "\r\n                        Scorpio - Jupiter is beneficial. The Sun and the Moon\r\n                        produce Rajayoga. Mercury and Venus are evil. Jupiter,\r\n                        even if be becomes a maraka, does not inflict death. Mercury\r\n                        and other evil planets, when they get death-inlflicting powers,\r\n                        do not certainly spare the native.\r\n                        ",
            ZodiacName.Sagittarius => "\r\n                        Sagittarius - Mars is the best planet and in conjunction\r\n                        with Jupiter, produces much good. The Sun and Mars also\r\n                        produce good. Venus is evil. When the Sun and Mars\r\n                        combine together they produce Rajayoga. Saturn does not\r\n                        bring about death even when he is a maraka. But Venus\r\n                        causes death when be gets jurisdiction as a maraka planet.\r\n                        ",
            ZodiacName.Capricorn => "\r\n                        Capricornus - Venus is the most powerful planet and in\r\n                        conjunction with Mercury produces Rajayoga. Mars, Jupiter\r\n                        and the Moon are evil.\r\n                        ",
            ZodiacName.Aquarius => "\r\n                        Aquarius - Venus alone is auspicious. The combination of\r\n                        Venus and Mars causes Rajayoga. Jupiter and the Moon are\r\n                        evil.\r\n                        ",
            ZodiacName.Pisces => "\r\n                        Pisces - The Moon and Mars are auspicious. Mars is\r\n                        most powerful. Mars with the Moon or Jupiter causes Rajayoga.\r\n                        Saturn, Venus, the Sun and Mercury are evil. Mars\r\n                        himself does not kill the person even if he is a maraka.\r\n                        ",
            _ => throw new ArgumentOutOfRangeException("acesendatName", acesendatName, null),
        };
    }

    public static string GetSignDescription(ZodiacName zodiacName)
    {
        return zodiacName switch
        {
            ZodiacName.Aries => "movable, odd, masculine, cruel, fiery, of short ascension, rising by hinder part, powerful during the night",
            ZodiacName.Taurus => "fixed, even, feminine, mild,earthy, fruitful, of short ascension, rising by hinder part",
            ZodiacName.Gemini => "common, odd, masculine, cruel, airy, barren, of short ascension, rising by the head.",
            ZodiacName.Cancer => "even, movable, feminine, mild, watery, of long ascension, rising by the hinder part and fruitful.",
            ZodiacName.Leo => "fixed, odd, masculine, cruel, fiery, of long ascension, barren, rising by the head.",
            ZodiacName.Virgo => "common, even, feminine, mild, earthy, of long ascension, rising by the head.",
            ZodiacName.Libra => "movable, odd, masculine, cruel, airy, of long ascension, rising by the head.",
            ZodiacName.Scorpio => "fixed, even, feminine, mild, watery, of long ascension, rising by the head.",
            ZodiacName.Sagittarius => "common, odd, masculine, cruel, fiery, of long ascension, rising by the hinder part.",
            ZodiacName.Capricorn => "movable, even, feminine, mild, earthy, of long ascension, rising by hinder part",
            ZodiacName.Aquarius => "fixed, odd, masculine, cruel, fruitful, airy, of short ascension, rising by the head.",
            ZodiacName.Pisces => "common, feminine, water, even, mild, of short ascension, rising by head and hinder part.",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static string GetHouseType(int houseNumber)
    {
        string text = "";
        switch (houseNumber)
        {
            case 1:
            case 4:
            case 7:
            case 10:
                text += "Quadrants (kendras)";
                break;
            case 5:
            case 9:
                text += "Trines (Trikonas)";
                break;
        }

        switch (houseNumber)
        {
            case 2:
            case 5:
            case 8:
            case 11:
                text += "Cadent (Panaparas)";
                break;
            case 3:
            case 6:
            case 9:
            case 12:
                text += "Succeedent (Apoklimas)";
                break;
        }

        if (houseNumber == 3 || houseNumber == 6 || (uint)(houseNumber - 10) <= 1u)
        {
            text += "Upachayas";
        }

        return text;
    }

    public static string GetPlanetInfo(PlanetName lordOfHouse)
    {
        return lordOfHouse.Name switch
        {
            PlanetName.PlanetNameEnum.Sun => "Father, masculine, malefic, copper colour, philosophical tendency, royal, ego, sons, patrimony, self reliance, political power, windy and bilious temperament, month, places of worship, money-lenders, goldsmith, bones, fires, coronation chambers, doctoring capacity",
            PlanetName.PlanetNameEnum.Moon => "Mother, feminine, mind, benefic when waxing, malefic when waning, white colour, women, sea-men, pearls, gems, water, fishermen, stubbornness, romances, bath-rooms, blood, popularity, human responsibilities",
            PlanetName.PlanetNameEnum.Mars => "Brothers, masculine, blood-red colour, malefic, refined taste, base metals, vegetation, rotten things, orators, ambassadors, military activities, commerce, aerial journeys, weaving, public speakers.",
            PlanetName.PlanetNameEnum.Mercury => "Profession, benefic if well associated, hermaphrodite, green colour, mercantile activity, public speakers, cold nervous, intelligence",
            PlanetName.PlanetNameEnum.Jupiter => "Children, masculine, benefic, bright yellow colour, devotion, truthfulness, religious fervour, philosophical wisdom, corpulence",
            PlanetName.PlanetNameEnum.Venus => "Wife, feminine, benefic, mixture of all colours, love affairs, sensual pleasure, family bliss, harems of ill-fame, vitality",
            PlanetName.PlanetNameEnum.Saturn => "Longevity, malefic, hermaphrodite, dark colour, stubbornness, impetuosity, demoralisation, windy diseases, despondency, gambling",
            PlanetName.PlanetNameEnum.Rahu => "Maternal relations, malefic, feminine, renunciation, corruption, epidemics",
            PlanetName.PlanetNameEnum.Ketu => "Paternal relations, Hermaphrodite, malefic, religious, sectarian principles, pride, selfishness, occultism, mendicancy",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static bool IsPlanetBeneficToLagna(PlanetName planetName, ZodiacName lagna)
    {
        return lagna switch
        {
            ZodiacName.Aries => planetName == PlanetName.Sun || planetName == PlanetName.Mars || planetName == PlanetName.Jupiter,
            ZodiacName.Taurus => planetName == PlanetName.Sun || planetName == PlanetName.Mars || planetName == PlanetName.Mercury || planetName == PlanetName.Saturn,
            ZodiacName.Gemini => planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            ZodiacName.Cancer => planetName == PlanetName.Mars || planetName == PlanetName.Jupiter,
            ZodiacName.Leo => planetName == PlanetName.Sun || planetName == PlanetName.Mars,
            ZodiacName.Virgo => planetName == PlanetName.Venus,
            ZodiacName.Libra => planetName == PlanetName.Mercury || planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            ZodiacName.Scorpio => planetName == PlanetName.Jupiter || planetName == PlanetName.Sun || planetName == PlanetName.Moon,
            ZodiacName.Sagittarius => planetName == PlanetName.Sun || planetName == PlanetName.Mars,
            ZodiacName.Capricorn => planetName == PlanetName.Mercury || planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            ZodiacName.Aquarius => planetName == PlanetName.Venus || planetName == PlanetName.Mars || planetName == PlanetName.Sun || planetName == PlanetName.Saturn,
            ZodiacName.Pisces => planetName == PlanetName.Mars || planetName == PlanetName.Moon,
            _ => throw new InvalidOperationException(),
        };
    }

    public static bool IsPlanetMaleficToLagna(PlanetName planetName, ZodiacName lagna)
    {
        return lagna switch
        {
            ZodiacName.Aries => planetName == PlanetName.Venus || planetName == PlanetName.Mercury || planetName == PlanetName.Saturn,
            ZodiacName.Taurus => planetName == PlanetName.Moon || planetName == PlanetName.Jupiter || planetName == PlanetName.Venus,
            ZodiacName.Gemini => planetName == PlanetName.Sun || planetName == PlanetName.Mars || planetName == PlanetName.Jupiter,
            ZodiacName.Cancer => planetName == PlanetName.Mercury || planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            ZodiacName.Leo => planetName == PlanetName.Mercury || planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            ZodiacName.Virgo => planetName == PlanetName.Mars || planetName == PlanetName.Moon || planetName == PlanetName.Jupiter,
            ZodiacName.Libra => planetName == PlanetName.Sun || planetName == PlanetName.Moon || planetName == PlanetName.Jupiter,
            ZodiacName.Scorpio => planetName == PlanetName.Mercury || planetName == PlanetName.Saturn,
            ZodiacName.Sagittarius => planetName == PlanetName.Saturn || planetName == PlanetName.Venus || planetName == PlanetName.Mercury,
            ZodiacName.Capricorn => planetName == PlanetName.Moon || planetName == PlanetName.Mars || planetName == PlanetName.Jupiter,
            ZodiacName.Aquarius => planetName == PlanetName.Jupiter || planetName == PlanetName.Moon,
            ZodiacName.Pisces => planetName == PlanetName.Sun || planetName == PlanetName.Mercury || planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            _ => throw new InvalidOperationException(),
        };
    }

    public static bool IsPlanetYogakarakaToLagna(PlanetName planetName, ZodiacName lagna)
    {
        return lagna switch
        {
            ZodiacName.Aries => planetName == PlanetName.Sun,
            ZodiacName.Taurus => planetName == PlanetName.Saturn,
            ZodiacName.Gemini => planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            ZodiacName.Cancer => planetName == PlanetName.Mars,
            ZodiacName.Leo => planetName == PlanetName.Mars,
            ZodiacName.Virgo => planetName == PlanetName.Mercury || planetName == PlanetName.Venus,
            ZodiacName.Libra => planetName == PlanetName.Moon || planetName == PlanetName.Mercury || planetName == PlanetName.Saturn,
            ZodiacName.Scorpio => planetName == PlanetName.Sun || planetName == PlanetName.Moon,
            ZodiacName.Sagittarius => planetName == PlanetName.Sun || planetName == PlanetName.Mars,
            ZodiacName.Capricorn => planetName == PlanetName.Mercury || planetName == PlanetName.Venus,
            ZodiacName.Aquarius => planetName == PlanetName.Mars || planetName == PlanetName.Venus,
            ZodiacName.Pisces => planetName == PlanetName.Mars || planetName == PlanetName.Jupiter || planetName == PlanetName.Moon,
            _ => throw new InvalidOperationException(),
        };
    }

    public static bool IsPlanetMarakaToLagna(PlanetName planetName, ZodiacName lagna)
    {
        return lagna switch
        {
            ZodiacName.Aries => planetName == PlanetName.Mercury || planetName == PlanetName.Saturn,
            ZodiacName.Taurus => planetName == PlanetName.Jupiter || planetName == PlanetName.Venus,
            ZodiacName.Gemini => planetName == PlanetName.Mars || planetName == PlanetName.Jupiter,
            ZodiacName.Cancer => planetName == PlanetName.Mercury || planetName == PlanetName.Venus,
            ZodiacName.Leo => planetName == PlanetName.Mercury || planetName == PlanetName.Venus,
            ZodiacName.Virgo => planetName == PlanetName.Mars || planetName == PlanetName.Jupiter,
            ZodiacName.Libra => planetName == PlanetName.Jupiter,
            ZodiacName.Scorpio => planetName == PlanetName.Mercury || planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            ZodiacName.Sagittarius => planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            ZodiacName.Capricorn => planetName == PlanetName.Mars || planetName == PlanetName.Jupiter,
            ZodiacName.Aquarius => planetName == PlanetName.Mars,
            ZodiacName.Pisces => planetName == PlanetName.Mercury || planetName == PlanetName.Venus || planetName == PlanetName.Saturn,
            _ => throw new InvalidOperationException(),
        };
    }

    public static bool IsPlanetInOwnHouse(Time time, PlanetName planetName)
    {
        bool flag = planetName == PlanetName.Rahu || planetName == PlanetName.Ketu;
        int housePlanetIsIn = GetHousePlanetIsIn(time, planetName);
        PlanetToSignRelationship planetToSignRelationship = ((!flag) ? GetPlanetRelationshipWithHouse((HouseName)housePlanetIsIn, planetName, time) : ((PlanetToSignRelationship)0));
        if (planetToSignRelationship == PlanetToSignRelationship.OwnVarga)
        {
            return true;
        }

        return false;
    }

    public static bool IsPlanetSameHouseWithHouseLord(Time birthTime, int houseNumber, PlanetName planet)
    {
        PlanetName lordOfHouse = GetLordOfHouse((HouseName)houseNumber, birthTime);
        int housePlanetIsIn = GetHousePlanetIsIn(birthTime, lordOfHouse);
        int housePlanetIsIn2 = GetHousePlanetIsIn(birthTime, planet);
        if (housePlanetIsIn2 == housePlanetIsIn)
        {
            return true;
        }

        return false;
    }

    public static Angle GetAyanamsa(Time time)
    {
        int year = LmtToUtc(time).Year;
        return new Angle(0.0, 0.0, (long)Math.Round((double)(year - 397) * 50.3333333333));
    }

    public static Angle GetPlanetSayanaLongitude(Time time, PlanetName planetName)
    {
        int ipl = 0;
        int iflag = 2;
        double[] array = new double[6];
        string serr = "";
        SwissEph swissEph = new SwissEph();
        double tjd = TimeToEphemerisTime(time);
        if (planetName == PlanetName.Sun)
        {
            ipl = 0;
        }
        else if (planetName == PlanetName.Moon)
        {
            ipl = 1;
        }
        else if (planetName == PlanetName.Mercury)
        {
            ipl = 2;
        }
        else if (planetName == PlanetName.Venus)
        {
            ipl = 3;
        }
        else if (planetName == PlanetName.Mars)
        {
            ipl = 4;
        }
        else if (planetName == PlanetName.Jupiter)
        {
            ipl = 5;
        }
        else if (planetName == PlanetName.Saturn)
        {
            ipl = 6;
        }
        else if (planetName == PlanetName.Uranus)
        {
            ipl = 7;
        }
        else if (planetName == PlanetName.Neptune)
        {
            ipl = 8;
        }
        else if (planetName == PlanetName.Pluto)
        {
            ipl = 9;
        }
        else if (planetName == PlanetName.Rahu)
        {
            ipl = 10;
        }
        else if (planetName == PlanetName.Ketu)
        {
            ipl = 10;
        }

        int num = swissEph.swe_calc(tjd, ipl, iflag, array, ref serr);
        return new Angle(array[0], 0.0, 0L);
    }

    public static Angle GetPlanetSayanaLatitude(Time time, PlanetName planetName)
    {
        int ipl = 0;
        int iflag = 2;
        double[] array = new double[6];
        string serr = "";
        SwissEph swissEph = new SwissEph();
        double tjd = TimeToEphemerisTime(time);
        if (planetName == PlanetName.Sun)
        {
            ipl = 0;
        }
        else if (planetName == PlanetName.Moon)
        {
            ipl = 1;
        }
        else if (planetName == PlanetName.Mars)
        {
            ipl = 4;
        }
        else if (planetName == PlanetName.Mercury)
        {
            ipl = 2;
        }
        else if (planetName == PlanetName.Jupiter)
        {
            ipl = 5;
        }
        else if (planetName == PlanetName.Venus)
        {
            ipl = 3;
        }
        else if (planetName == PlanetName.Saturn)
        {
            ipl = 6;
        }
        else if (planetName == PlanetName.Rahu)
        {
            ipl = 10;
        }
        else if (planetName == PlanetName.Ketu)
        {
            ipl = 10;
        }

        int num = swissEph.swe_calc(tjd, ipl, iflag, array, ref serr);
        return new Angle(array[1], 0.0, 0L);
    }

    public static double GetPlanetSpeed(Time time, PlanetName planetName)
    {
        int ipl = 0;
        int iflag = 258;
        double[] array = new double[6];
        string serr = "";
        SwissEph swissEph = new SwissEph();
        double tjd = TimeToEphemerisTime(time);
        if (planetName == PlanetName.Sun)
        {
            ipl = 0;
        }
        else if (planetName == PlanetName.Moon)
        {
            ipl = 1;
        }
        else if (planetName == PlanetName.Mars)
        {
            ipl = 4;
        }
        else if (planetName == PlanetName.Mercury)
        {
            ipl = 2;
        }
        else if (planetName == PlanetName.Jupiter)
        {
            ipl = 5;
        }
        else if (planetName == PlanetName.Venus)
        {
            ipl = 3;
        }
        else if (planetName == PlanetName.Saturn)
        {
            ipl = 6;
        }
        else if (planetName == PlanetName.Rahu)
        {
            ipl = 10;
        }
        else if (planetName == PlanetName.Ketu)
        {
            ipl = 10;
        }

        int num = swissEph.swe_calc(tjd, ipl, iflag, array, ref serr);
        return array[3];
    }

    public static PlanetConstellation GetConstellationAtLongitude(Angle planetLongitude)
    {
        double totalMinutes = planetLongitude.TotalMinutes;
        double num = totalMinutes / 800.0;
        int constellationNumber = (int)Math.Ceiling(num);
        double num2 = num - Math.Floor(num);
        int quarter = ((num2 >= 0.0 && num2 <= 0.25) ? 1 : ((num2 > 0.25 && num2 <= 0.5) ? 2 : ((num2 > 0.5 && num2 <= 0.75) ? 3 : ((num2 > 0.75 && num2 <= 1.0) ? 4 : 0))));
        double minutes = num2 * 800.0;
        Angle degreeInConstellation = new Angle(0.0, minutes, 0L);
        return new PlanetConstellation(constellationNumber, quarter, degreeInConstellation);
    }

    public static ZodiacSign GetZodiacSignAtLongitude(Angle longitude)
    {
        double num = longitude.TotalDegrees % 360.0 / 30.0;
        double num2 = num - Math.Truncate(num);
        double value = num2 * 30.0;
        double num3 = Math.Round(value, 4);
        if (num3 == 0.0)
        {
            num3 = 30.0;
        }

        int num4 = (int)Math.Ceiling(num);
        ZodiacName zodiacName = (ZodiacName)num4;
        ZodiacName signName = ((num <= 0.0) ? ZodiacName.Pisces : zodiacName);
        Angle degreesInSign = Angle.FromDegrees(Math.Abs(num3));
        return new ZodiacSign(signName, degreesInSign);
    }

    public static Angle GetLongitudeAtZodiacSign(ZodiacSign zodiacSign)
    {
        int signName = (int)zodiacSign.GetSignName();
        int num = signName - 1;
        double num2 = 30.0;
        Angle angle = Angle.FromDegrees(num2 * (double)num);
        return angle + zodiacSign.GetDegreesInSign();
    }

    public static DayOfWeek GetDayOfWeek(Time time)
    {
        Time sunriseTime = GetSunriseTime(time);
        string value = time.GetLmtDateTimeOffset().DayOfWeek.ToString();
        Enum.TryParse<DayOfWeek>(value, out var result);
        return result;
    }

    public static PlanetName GetLordOfHora(int hora, DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Sunday:
                switch (hora)
                {
                    case 1:
                        return PlanetName.Sun;
                    case 2:
                        return PlanetName.Venus;
                    case 3:
                        return PlanetName.Mercury;
                    case 4:
                        return PlanetName.Moon;
                    case 5:
                        return PlanetName.Saturn;
                    case 6:
                        return PlanetName.Jupiter;
                    case 7:
                        return PlanetName.Mars;
                    case 8:
                        return PlanetName.Sun;
                    case 9:
                        return PlanetName.Venus;
                    case 10:
                        return PlanetName.Mercury;
                    case 11:
                        return PlanetName.Moon;
                    case 12:
                        return PlanetName.Saturn;
                    case 13:
                        return PlanetName.Jupiter;
                    case 14:
                        return PlanetName.Mars;
                    case 15:
                        return PlanetName.Sun;
                    case 16:
                        return PlanetName.Venus;
                    case 17:
                        return PlanetName.Mercury;
                    case 18:
                        return PlanetName.Moon;
                    case 19:
                        return PlanetName.Saturn;
                    case 20:
                        return PlanetName.Jupiter;
                    case 21:
                        return PlanetName.Mars;
                    case 22:
                        return PlanetName.Sun;
                    case 23:
                        return PlanetName.Venus;
                    case 24:
                        return PlanetName.Mercury;
                }

                break;
            case DayOfWeek.Monday:
                switch (hora)
                {
                    case 1:
                        return PlanetName.Moon;
                    case 2:
                        return PlanetName.Saturn;
                    case 3:
                        return PlanetName.Jupiter;
                    case 4:
                        return PlanetName.Mars;
                    case 5:
                        return PlanetName.Sun;
                    case 6:
                        return PlanetName.Venus;
                    case 7:
                        return PlanetName.Mercury;
                    case 8:
                        return PlanetName.Moon;
                    case 9:
                        return PlanetName.Saturn;
                    case 10:
                        return PlanetName.Jupiter;
                    case 11:
                        return PlanetName.Mars;
                    case 12:
                        return PlanetName.Sun;
                    case 13:
                        return PlanetName.Venus;
                    case 14:
                        return PlanetName.Mercury;
                    case 15:
                        return PlanetName.Moon;
                    case 16:
                        return PlanetName.Saturn;
                    case 17:
                        return PlanetName.Jupiter;
                    case 18:
                        return PlanetName.Mars;
                    case 19:
                        return PlanetName.Sun;
                    case 20:
                        return PlanetName.Venus;
                    case 21:
                        return PlanetName.Mercury;
                    case 22:
                        return PlanetName.Moon;
                    case 23:
                        return PlanetName.Saturn;
                    case 24:
                        return PlanetName.Jupiter;
                }

                break;
            case DayOfWeek.Tuesday:
                switch (hora)
                {
                    case 1:
                        return PlanetName.Mars;
                    case 2:
                        return PlanetName.Sun;
                    case 3:
                        return PlanetName.Venus;
                    case 4:
                        return PlanetName.Mercury;
                    case 5:
                        return PlanetName.Moon;
                    case 6:
                        return PlanetName.Saturn;
                    case 7:
                        return PlanetName.Jupiter;
                    case 8:
                        return PlanetName.Mars;
                    case 9:
                        return PlanetName.Sun;
                    case 10:
                        return PlanetName.Venus;
                    case 11:
                        return PlanetName.Mercury;
                    case 12:
                        return PlanetName.Moon;
                    case 13:
                        return PlanetName.Saturn;
                    case 14:
                        return PlanetName.Jupiter;
                    case 15:
                        return PlanetName.Mars;
                    case 16:
                        return PlanetName.Sun;
                    case 17:
                        return PlanetName.Venus;
                    case 18:
                        return PlanetName.Mercury;
                    case 19:
                        return PlanetName.Moon;
                    case 20:
                        return PlanetName.Saturn;
                    case 21:
                        return PlanetName.Jupiter;
                    case 22:
                        return PlanetName.Mars;
                    case 23:
                        return PlanetName.Sun;
                    case 24:
                        return PlanetName.Venus;
                }

                break;
            case DayOfWeek.Wednesday:
                switch (hora)
                {
                    case 1:
                        return PlanetName.Mercury;
                    case 2:
                        return PlanetName.Moon;
                    case 3:
                        return PlanetName.Saturn;
                    case 4:
                        return PlanetName.Jupiter;
                    case 5:
                        return PlanetName.Mars;
                    case 6:
                        return PlanetName.Sun;
                    case 7:
                        return PlanetName.Venus;
                    case 8:
                        return PlanetName.Mercury;
                    case 9:
                        return PlanetName.Moon;
                    case 10:
                        return PlanetName.Saturn;
                    case 11:
                        return PlanetName.Jupiter;
                    case 12:
                        return PlanetName.Mars;
                    case 13:
                        return PlanetName.Sun;
                    case 14:
                        return PlanetName.Venus;
                    case 15:
                        return PlanetName.Mercury;
                    case 16:
                        return PlanetName.Moon;
                    case 17:
                        return PlanetName.Saturn;
                    case 18:
                        return PlanetName.Jupiter;
                    case 19:
                        return PlanetName.Mars;
                    case 20:
                        return PlanetName.Sun;
                    case 21:
                        return PlanetName.Venus;
                    case 22:
                        return PlanetName.Mercury;
                    case 23:
                        return PlanetName.Moon;
                    case 24:
                        return PlanetName.Saturn;
                }

                break;
            case DayOfWeek.Thursday:
                switch (hora)
                {
                    case 1:
                        return PlanetName.Jupiter;
                    case 2:
                        return PlanetName.Mars;
                    case 3:
                        return PlanetName.Sun;
                    case 4:
                        return PlanetName.Venus;
                    case 5:
                        return PlanetName.Mercury;
                    case 6:
                        return PlanetName.Moon;
                    case 7:
                        return PlanetName.Saturn;
                    case 8:
                        return PlanetName.Jupiter;
                    case 9:
                        return PlanetName.Mars;
                    case 10:
                        return PlanetName.Sun;
                    case 11:
                        return PlanetName.Venus;
                    case 12:
                        return PlanetName.Mercury;
                    case 13:
                        return PlanetName.Moon;
                    case 14:
                        return PlanetName.Saturn;
                    case 15:
                        return PlanetName.Jupiter;
                    case 16:
                        return PlanetName.Mars;
                    case 17:
                        return PlanetName.Sun;
                    case 18:
                        return PlanetName.Venus;
                    case 19:
                        return PlanetName.Mercury;
                    case 20:
                        return PlanetName.Moon;
                    case 21:
                        return PlanetName.Saturn;
                    case 22:
                        return PlanetName.Jupiter;
                    case 23:
                        return PlanetName.Mars;
                    case 24:
                        return PlanetName.Sun;
                }

                break;
            case DayOfWeek.Friday:
                switch (hora)
                {
                    case 1:
                        return PlanetName.Venus;
                    case 2:
                        return PlanetName.Mercury;
                    case 3:
                        return PlanetName.Moon;
                    case 4:
                        return PlanetName.Saturn;
                    case 5:
                        return PlanetName.Jupiter;
                    case 6:
                        return PlanetName.Mars;
                    case 7:
                        return PlanetName.Sun;
                    case 8:
                        return PlanetName.Venus;
                    case 9:
                        return PlanetName.Mercury;
                    case 10:
                        return PlanetName.Moon;
                    case 11:
                        return PlanetName.Saturn;
                    case 12:
                        return PlanetName.Jupiter;
                    case 13:
                        return PlanetName.Mars;
                    case 14:
                        return PlanetName.Sun;
                    case 15:
                        return PlanetName.Venus;
                    case 16:
                        return PlanetName.Mercury;
                    case 17:
                        return PlanetName.Moon;
                    case 18:
                        return PlanetName.Saturn;
                    case 19:
                        return PlanetName.Jupiter;
                    case 20:
                        return PlanetName.Mars;
                    case 21:
                        return PlanetName.Sun;
                    case 22:
                        return PlanetName.Venus;
                    case 23:
                        return PlanetName.Mercury;
                    case 24:
                        return PlanetName.Moon;
                }

                break;
            case DayOfWeek.Saturday:
                switch (hora)
                {
                    case 1:
                        return PlanetName.Saturn;
                    case 2:
                        return PlanetName.Jupiter;
                    case 3:
                        return PlanetName.Mars;
                    case 4:
                        return PlanetName.Sun;
                    case 5:
                        return PlanetName.Venus;
                    case 6:
                        return PlanetName.Mercury;
                    case 7:
                        return PlanetName.Moon;
                    case 8:
                        return PlanetName.Saturn;
                    case 9:
                        return PlanetName.Jupiter;
                    case 10:
                        return PlanetName.Mars;
                    case 11:
                        return PlanetName.Sun;
                    case 12:
                        return PlanetName.Venus;
                    case 13:
                        return PlanetName.Mercury;
                    case 14:
                        return PlanetName.Moon;
                    case 15:
                        return PlanetName.Saturn;
                    case 16:
                        return PlanetName.Jupiter;
                    case 17:
                        return PlanetName.Mars;
                    case 18:
                        return PlanetName.Sun;
                    case 19:
                        return PlanetName.Venus;
                    case 20:
                        return PlanetName.Mercury;
                    case 21:
                        return PlanetName.Moon;
                    case 22:
                        return PlanetName.Saturn;
                    case 23:
                        return PlanetName.Jupiter;
                    case 24:
                        return PlanetName.Mars;
                }

                break;
        }

        throw new Exception("Did not find hora, something wrong!");
    }

    public static Angle GetHouseJunctionPoint(Angle previousHouse, Angle nextHouse)
    {
        Angle angle = previousHouse + nextHouse;
        if (nextHouse < previousHouse)
        {
            angle += Angle.Degrees360;
            Angle angle2 = angle.Divide(2.0);
            return angle2 - Angle.Degrees360;
        }

        return angle.Divide(2.0);
    }

    public static PlanetName GetLordOfZodiacSign(ZodiacName signName)
    {
        switch (signName)
        {
            case ZodiacName.Aries:
            case ZodiacName.Scorpio:
                return PlanetName.Mars;
            case ZodiacName.Taurus:
            case ZodiacName.Libra:
                return PlanetName.Venus;
            case ZodiacName.Gemini:
            case ZodiacName.Virgo:
                return PlanetName.Mercury;
            case ZodiacName.Cancer:
                return PlanetName.Moon;
            case ZodiacName.Leo:
                return PlanetName.Sun;
            case ZodiacName.Sagittarius:
            case ZodiacName.Pisces:
                return PlanetName.Jupiter;
            case ZodiacName.Capricorn:
            case ZodiacName.Aquarius:
                return PlanetName.Saturn;
            default:
                throw new Exception("Lord of sign not found, error!");
        }
    }

    public static ZodiacName GetNextZodiacSign(ZodiacName inputSign)
    {
        int num = (int)inputSign;
        if (num == 12)
        {
            return ZodiacName.Aries;
        }

        return (ZodiacName)(num + 1);
    }

    public static int GetNextHouseNumber(int inputHouseNumber)
    {
        if (inputHouseNumber == 12)
        {
            return 1;
        }

        return inputHouseNumber + 1;
    }

    public static ZodiacSign GetPlanetExaltationPoint(PlanetName planetName)
    {
        if (planetName == PlanetName.Sun)
        {
            return new ZodiacSign(ZodiacName.Aries, Angle.FromDegrees(10.0));
        }

        if (planetName == PlanetName.Moon)
        {
            return new ZodiacSign(ZodiacName.Taurus, Angle.FromDegrees(3.0));
        }

        if (planetName == PlanetName.Mars)
        {
            return new ZodiacSign(ZodiacName.Capricorn, Angle.FromDegrees(28.0));
        }

        if (planetName == PlanetName.Mercury)
        {
            return new ZodiacSign(ZodiacName.Virgo, Angle.FromDegrees(15.0));
        }

        if (planetName == PlanetName.Jupiter)
        {
            return new ZodiacSign(ZodiacName.Cancer, Angle.FromDegrees(5.0));
        }

        if (planetName == PlanetName.Venus)
        {
            return new ZodiacSign(ZodiacName.Pisces, Angle.FromDegrees(27.0));
        }

        if (planetName == PlanetName.Saturn)
        {
            return new ZodiacSign(ZodiacName.Libra, Angle.FromDegrees(20.0));
        }

        if (planetName == PlanetName.Rahu)
        {
            return new ZodiacSign(ZodiacName.Taurus, Angle.FromDegrees(20.0));
        }

        if (!(planetName == PlanetName.Ketu))
        {
            throw new Exception("Planet exaltation point not found, error!");
        }

        return new ZodiacSign(ZodiacName.Scorpio, Angle.FromDegrees(20.0));
    }

    public static ZodiacSign GetPlanetDebilitationPoint(PlanetName planetName)
    {
        if (planetName == PlanetName.Sun)
        {
            return new ZodiacSign(ZodiacName.Libra, Angle.FromDegrees(10.0));
        }

        if (planetName == PlanetName.Moon)
        {
            return new ZodiacSign(ZodiacName.Scorpio, Angle.FromDegrees(0.0));
        }

        if (planetName == PlanetName.Mars)
        {
            return new ZodiacSign(ZodiacName.Cancer, Angle.FromDegrees(28.0));
        }

        if (planetName == PlanetName.Mercury)
        {
            return new ZodiacSign(ZodiacName.Pisces, Angle.FromDegrees(15.0));
        }

        if (planetName == PlanetName.Jupiter)
        {
            return new ZodiacSign(ZodiacName.Capricorn, Angle.FromDegrees(5.0));
        }

        if (planetName == PlanetName.Venus)
        {
            return new ZodiacSign(ZodiacName.Virgo, Angle.FromDegrees(27.0));
        }

        if (planetName == PlanetName.Saturn)
        {
            return new ZodiacSign(ZodiacName.Aries, Angle.FromDegrees(20.0));
        }

        if (planetName == PlanetName.Rahu)
        {
            return new ZodiacSign(ZodiacName.Scorpio, Angle.FromDegrees(20.0));
        }

        if (!(planetName == PlanetName.Ketu))
        {
            throw new Exception("Planet debilitation point not found, error!");
        }

        return new ZodiacSign(ZodiacName.Taurus, Angle.FromDegrees(20.0));
    }

    public static bool IsEvenSign(ZodiacName planetSignName)
    {
        if (planetSignName == ZodiacName.Taurus || planetSignName == ZodiacName.Cancer || planetSignName == ZodiacName.Virgo || planetSignName == ZodiacName.Scorpio || planetSignName == ZodiacName.Capricorn || planetSignName == ZodiacName.Pisces)
        {
            return true;
        }

        return false;
    }

    public static bool IsOddSign(ZodiacName planetSignName)
    {
        if (planetSignName == ZodiacName.Aries || planetSignName == ZodiacName.Gemini || planetSignName == ZodiacName.Leo || planetSignName == ZodiacName.Libra || planetSignName == ZodiacName.Sagittarius || planetSignName == ZodiacName.Aquarius)
        {
            return true;
        }

        return false;
    }

    public static bool IsFixedSign(ZodiacName sunSign)
    {
        switch (sunSign)
        {
            case ZodiacName.Taurus:
            case ZodiacName.Leo:
            case ZodiacName.Scorpio:
            case ZodiacName.Aquarius:
                return true;
            default:
                return false;
        }
    }

    public static bool IsMovableSign(ZodiacName sunSign)
    {
        switch (sunSign)
        {
            case ZodiacName.Aries:
            case ZodiacName.Cancer:
            case ZodiacName.Libra:
            case ZodiacName.Capricorn:
                return true;
            default:
                return false;
        }
    }

    public static bool IsCommonSign(ZodiacName sunSign)
    {
        switch (sunSign)
        {
            case ZodiacName.Gemini:
            case ZodiacName.Virgo:
            case ZodiacName.Sagittarius:
            case ZodiacName.Pisces:
                return true;
            default:
                return false;
        }
    }

    public static PlanetToPlanetRelationship GetPlanetPermanentRelationshipWithPlanet(PlanetName mainPlanet, PlanetName secondaryPlanet)
    {
        if (mainPlanet == secondaryPlanet)
        {
            return PlanetToPlanetRelationship.SamePlanet;
        }

        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        if (mainPlanet == PlanetName.Sun)
        {
            List<PlanetName> list = new List<PlanetName>
                {
                    PlanetName.Moon,
                    PlanetName.Mars,
                    PlanetName.Jupiter
                };
            List<PlanetName> list2 = new List<PlanetName> { PlanetName.Mercury };
            List<PlanetName> list3 = new List<PlanetName>
                {
                    PlanetName.Venus,
                    PlanetName.Saturn
                };
            flag3 = list.Contains(secondaryPlanet);
            flag2 = list2.Contains(secondaryPlanet);
            flag = list3.Contains(secondaryPlanet);
        }

        if (mainPlanet == PlanetName.Moon)
        {
            List<PlanetName> list4 = new List<PlanetName>
                {
                    PlanetName.Sun,
                    PlanetName.Mercury
                };
            List<PlanetName> list5 = new List<PlanetName>
                {
                    PlanetName.Mars,
                    PlanetName.Jupiter,
                    PlanetName.Venus,
                    PlanetName.Saturn
                };
            List<PlanetName> list6 = new List<PlanetName>();
            flag3 = list4.Contains(secondaryPlanet);
            flag2 = list5.Contains(secondaryPlanet);
            flag = list6.Contains(secondaryPlanet);
        }

        if (mainPlanet == PlanetName.Mars)
        {
            List<PlanetName> list7 = new List<PlanetName>
                {
                    PlanetName.Sun,
                    PlanetName.Moon,
                    PlanetName.Jupiter
                };
            List<PlanetName> list8 = new List<PlanetName>
                {
                    PlanetName.Venus,
                    PlanetName.Saturn
                };
            List<PlanetName> list9 = new List<PlanetName> { PlanetName.Mercury };
            flag3 = list7.Contains(secondaryPlanet);
            flag2 = list8.Contains(secondaryPlanet);
            flag = list9.Contains(secondaryPlanet);
        }

        if (mainPlanet == PlanetName.Mercury)
        {
            List<PlanetName> list10 = new List<PlanetName>
                {
                    PlanetName.Sun,
                    PlanetName.Venus
                };
            List<PlanetName> list11 = new List<PlanetName>
                {
                    PlanetName.Mars,
                    PlanetName.Jupiter,
                    PlanetName.Saturn
                };
            List<PlanetName> list12 = new List<PlanetName> { PlanetName.Moon };
            flag3 = list10.Contains(secondaryPlanet);
            flag2 = list11.Contains(secondaryPlanet);
            flag = list12.Contains(secondaryPlanet);
        }

        if (mainPlanet == PlanetName.Jupiter)
        {
            List<PlanetName> list13 = new List<PlanetName>
                {
                    PlanetName.Sun,
                    PlanetName.Moon,
                    PlanetName.Mars
                };
            List<PlanetName> list14 = new List<PlanetName> { PlanetName.Saturn };
            List<PlanetName> list15 = new List<PlanetName>
                {
                    PlanetName.Mercury,
                    PlanetName.Venus
                };
            flag3 = list13.Contains(secondaryPlanet);
            flag2 = list14.Contains(secondaryPlanet);
            flag = list15.Contains(secondaryPlanet);
        }

        if (mainPlanet == PlanetName.Venus)
        {
            List<PlanetName> list16 = new List<PlanetName>
                {
                    PlanetName.Mercury,
                    PlanetName.Saturn
                };
            List<PlanetName> list17 = new List<PlanetName>
                {
                    PlanetName.Mars,
                    PlanetName.Jupiter
                };
            List<PlanetName> list18 = new List<PlanetName>
                {
                    PlanetName.Sun,
                    PlanetName.Moon
                };
            flag3 = list16.Contains(secondaryPlanet);
            flag2 = list17.Contains(secondaryPlanet);
            flag = list18.Contains(secondaryPlanet);
        }

        if (mainPlanet == PlanetName.Saturn)
        {
            List<PlanetName> list19 = new List<PlanetName>
                {
                    PlanetName.Mercury,
                    PlanetName.Venus
                };
            List<PlanetName> list20 = new List<PlanetName> { PlanetName.Jupiter };
            List<PlanetName> list21 = new List<PlanetName>
                {
                    PlanetName.Sun,
                    PlanetName.Moon,
                    PlanetName.Mars
                };
            flag3 = list19.Contains(secondaryPlanet);
            flag2 = list20.Contains(secondaryPlanet);
            flag = list21.Contains(secondaryPlanet);
        }

        if (mainPlanet == PlanetName.Rahu || mainPlanet == PlanetName.Ketu)
        {
            throw new Exception("No Permenant Relation for Rahu and Ketu, use Temporary Relation!");
        }

        if (flag3)
        {
            return PlanetToPlanetRelationship.Friend;
        }

        if (flag2)
        {
            return PlanetToPlanetRelationship.Neutral;
        }

        if (!flag)
        {
            throw new Exception("planet permanent relationship not found, error!");
        }

        return PlanetToPlanetRelationship.Enemy;
    }

    public static DateTime ConvertJulianTimeToNormalTime(double julianTime)
    {
        SwissEph swissEph = new SwissEph();
        int gregflag = 1;
        int iyear = 0;
        int imonth = 0;
        int iday = 0;
        int ihour = 0;
        int imin = 0;
        double dsec = 0.0;
        swissEph.swe_jdut1_to_utc(julianTime, gregflag, ref iyear, ref imonth, ref iday, ref ihour, ref imin, ref dsec);
        return new DateTime(iyear, imonth, iday, ihour, imin, (int)dsec);
    }

    public static DateTimeOffset GetGreenwichTimeFromJulianDays(double julianTime)
    {
        SwissEph swissEph = new SwissEph();
        int gregflag = 1;
        int iyear = 0;
        int imonth = 0;
        int iday = 0;
        int ihour = 0;
        int imin = 0;
        double dsec = 0.0;
        swissEph.swe_jdut1_to_utc(julianTime, gregflag, ref iyear, ref imonth, ref iday, ref ihour, ref imin, ref dsec);
        DateTime dateTime = new DateTime(iyear, imonth, iday, ihour, imin, (int)dsec);
        return new DateTimeOffset(dateTime, new TimeSpan(0, 0, 0));
    }

    public static double ConvertLmtToJulian(Time time)
    {
        DateTimeOffset lmtDateTimeOffset = time.GetLmtDateTimeOffset();
        int year = lmtDateTimeOffset.Year;
        int month = lmtDateTimeOffset.Month;
        int day = lmtDateTimeOffset.Day;
        double totalHours = lmtDateTimeOffset.TimeOfDay.TotalHours;
        int gregflag = 1;
        SwissEph swissEph = new SwissEph();
        return swissEph.swe_julday(year, month, day, totalHours, gregflag);
    }

    public static double GetGreenwichLmtInJulianDays(Time time)
    {
        DateTimeOffset dateTimeOffset = time.GetLmtDateTimeOffset().ToUniversalTime();
        int year = dateTimeOffset.Year;
        int month = dateTimeOffset.Month;
        int day = dateTimeOffset.Day;
        double totalHours = dateTimeOffset.TimeOfDay.TotalHours;
        int gregflag = 1;
        SwissEph swissEph = new SwissEph();
        return swissEph.swe_julday(year, month, day, totalHours, gregflag);
    }

    public static double[] GetHouse1And10Longitudes(Time time)
    {
        GeoLocation geoLocation = time.GetGeoLocation();
        double tjd_ut = TimeToJulianDay(time);
        SwissEph swissEph = new SwissEph();
        double[] array = new double[13];
        double[] ascmc = new double[10];
        swissEph.swe_houses(tjd_ut, geoLocation.GetLatitude(), geoLocation.GetLongitude(), 'P', array, ascmc);
        return array;
    }

    public static DateTimeOffset LmtToUtc(Time time)
    {
        return time.GetLmtDateTimeOffset().ToUniversalTime();
    }

    public static int GetGocharaHouse(Time birthTime, Time currentTime, PlanetName planet)
    {
        ZodiacName moonSignName = GetMoonSignName(birthTime);
        ZodiacName signName = GetPlanetRasiSign(planet, currentTime).GetSignName();
        return CountFromSignToSign(moonSignName, signName);
    }

    public static bool IsGocharaObstructed(PlanetName planet, int gocharaHouse, Time birthTime, Time currentTime)
    {
        if (GetVedhanka(planet, gocharaHouse) == 0)
        {
            return false;
        }

        List<PlanetName> planetsInGocharaHouse = GetPlanetsInGocharaHouse(birthTime, currentTime, gocharaHouse);
        if (planet == PlanetName.Sun || planet == PlanetName.Saturn)
        {
            planetsInGocharaHouse.Remove(PlanetName.Sun);
            planetsInGocharaHouse.Remove(PlanetName.Saturn);
        }

        if (planet == PlanetName.Moon || planet == PlanetName.Mercury)
        {
            planetsInGocharaHouse.Remove(PlanetName.Moon);
            planetsInGocharaHouse.Remove(PlanetName.Mercury);
        }

        return planetsInGocharaHouse.Any();
    }

    public static List<PlanetName> GetPlanetsInGocharaHouse(Time birthTime, Time currentTime, int gocharaHouse)
    {
        int gocharaHouse2 = GetGocharaHouse(birthTime, currentTime, PlanetName.Sun);
        int gocharaHouse3 = GetGocharaHouse(birthTime, currentTime, PlanetName.Moon);
        int gocharaHouse4 = GetGocharaHouse(birthTime, currentTime, PlanetName.Mars);
        int gocharaHouse5 = GetGocharaHouse(birthTime, currentTime, PlanetName.Mercury);
        int gocharaHouse6 = GetGocharaHouse(birthTime, currentTime, PlanetName.Jupiter);
        int gocharaHouse7 = GetGocharaHouse(birthTime, currentTime, PlanetName.Venus);
        int gocharaHouse8 = GetGocharaHouse(birthTime, currentTime, PlanetName.Saturn);
        List<PlanetName> list = new List<PlanetName>();
        if (gocharaHouse2 == gocharaHouse)
        {
            list.Add(PlanetName.Sun);
        }

        if (gocharaHouse3 == gocharaHouse)
        {
            list.Add(PlanetName.Moon);
        }

        if (gocharaHouse4 == gocharaHouse)
        {
            list.Add(PlanetName.Mars);
        }

        if (gocharaHouse5 == gocharaHouse)
        {
            list.Add(PlanetName.Mercury);
        }

        if (gocharaHouse6 == gocharaHouse)
        {
            list.Add(PlanetName.Jupiter);
        }

        if (gocharaHouse7 == gocharaHouse)
        {
            list.Add(PlanetName.Venus);
        }

        if (gocharaHouse8 == gocharaHouse)
        {
            list.Add(PlanetName.Saturn);
        }

        return list;
    }

    public static int GetVedhanka(PlanetName planet, int house)
    {
        if (planet == PlanetName.Sun)
        {
            switch (house)
            {
                case 11:
                    return 5;
                case 3:
                    return 9;
                case 10:
                    return 4;
                case 6:
                    return 12;
                case 5:
                    return 11;
                case 9:
                    return 3;
                case 4:
                    return 10;
                case 12:
                    return 6;
            }
        }

        if (planet == PlanetName.Moon)
        {
            switch (house)
            {
                case 7:
                    return 2;
                case 1:
                    return 5;
                case 6:
                    return 12;
                case 11:
                    return 8;
                case 10:
                    return 4;
                case 3:
                    return 9;
                case 2:
                    return 7;
                case 5:
                    return 1;
                case 12:
                    return 6;
                case 8:
                    return 11;
                case 4:
                    return 10;
                case 9:
                    return 3;
            }
        }

        if (planet == PlanetName.Mars)
        {
            switch (house)
            {
                case 3:
                    return 12;
                case 11:
                    return 5;
                case 6:
                    return 9;
                case 12:
                    return 3;
                case 5:
                    return 11;
                case 9:
                    return 6;
            }
        }

        if (planet == PlanetName.Mercury)
        {
            switch (house)
            {
                case 2:
                    return 5;
                case 4:
                    return 3;
                case 6:
                    return 9;
                case 8:
                    return 1;
                case 10:
                    return 7;
                case 11:
                    return 12;
                case 5:
                    return 2;
                case 3:
                    return 4;
                case 9:
                    return 6;
                case 1:
                    return 8;
                case 7:
                    return 10;
                case 12:
                    return 11;
            }
        }

        if (planet == PlanetName.Jupiter)
        {
            switch (house)
            {
                case 2:
                    return 12;
                case 11:
                    return 8;
                case 9:
                    return 10;
                case 5:
                    return 4;
                case 7:
                    return 3;
                case 12:
                    return 2;
                case 8:
                    return 11;
                case 10:
                    return 9;
                case 4:
                    return 5;
                case 3:
                    return 7;
            }
        }

        if (planet == PlanetName.Venus)
        {
            switch (house)
            {
                case 1:
                    return 8;
                case 2:
                    return 7;
                case 3:
                    return 1;
                case 4:
                    return 10;
                case 5:
                    return 9;
                case 8:
                    return 5;
                case 9:
                    return 11;
                case 11:
                    return 6;
                case 12:
                    return 3;
            }

            switch (house)
            {
                case 8:
                    return 1;
                case 7:
                    return 2;
                case 1:
                    return 3;
                case 10:
                    return 4;
                case 9:
                    return 5;
                case 5:
                    return 8;
                case 11:
                    return 9;
                case 6:
                    return 11;
                case 3:
                    return 12;
            }
        }

        if (planet == PlanetName.Saturn)
        {
            switch (house)
            {
                case 3:
                    return 12;
                case 11:
                    return 5;
                case 6:
                    return 9;
                case 12:
                    return 3;
                case 5:
                    return 11;
                case 9:
                    return 6;
            }
        }

        if (planet == PlanetName.Rahu)
        {
            switch (house)
            {
                case 3:
                    return 12;
                case 11:
                    return 5;
                case 6:
                    return 9;
                case 12:
                    return 3;
                case 5:
                    return 11;
                case 9:
                    return 6;
            }
        }

        if (planet == PlanetName.Ketu)
        {
            switch (house)
            {
                case 3:
                    return 12;
                case 11:
                    return 5;
                case 6:
                    return 9;
                case 12:
                    return 3;
                case 5:
                    return 11;
                case 9:
                    return 6;
            }
        }

        return 0;
    }

    public static bool IsGocharaOccurring(Time birthTime, Time time, PlanetName planet, int gocharaHouse)
    {
        bool flag = GetGocharaHouse(birthTime, time, planet) == gocharaHouse;
        bool flag2 = !IsGocharaObstructed(planet, gocharaHouse, birthTime, time);
        return flag && flag2;
    }

    public static (EventNature eventNature, string desciption) GetPlanetDasaMajorPlanetAndMinorRelationship(PlanetName majorPlanet, PlanetName minorPlanet)
    {
        (EventNature, string) result = (EventNature.Empty, "");
        switch (majorPlanet.Name)
        {
            case PlanetName.PlanetNameEnum.Sun:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Unpleasantness with relatives and superiors, anxieties, headache, pain in the ear, some tendency to urinary or kidney troubles, sickness, fear from rulers and enemies, fear of death, loss of money, danger to father if the Sun is afflicted, stomachache and travels, gains through religious people, mental sufferings, a wandering life in a foreign country.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Winning favour from superiors, increase in business, fresh enterprises, troubles through women, eye troubles, many relatives and friends, indulgence in idle pastimes, jaundice and kindred ailments, new clothes and ornaments, will be happy, healthy, good meals, respect among relatives.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Rheumatic and similar troubles, quarrels, danger of enteric fever, dysentery, troubles to relatives, loss of money by thefts or wasteful expenses, failures, acquisition of wealth in the form of gold and gems, royal favour leading to prosperity, contraction and transmission of bilious and other diseases, mental worries, danger from fire, ill-health, loss of reputation, sorrow.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Gain in money, good reputation, acquisition of new clothes and ornaments, new education, trouble through relatives, mental distress, depression of spirits, waste of money and nervous weakness, no comforts, friends becoming enemies, much anxiety and fears, health bad, children ungrateful, disputes and trouble from ruler or judge, suffer disgrace, many short journeys and wanderings.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Benefits from friends and acquaintances, increase in education, employment in high circles, association with people of high rank, success through obstacles, birth of a child, wealth got through sons (if there is a son), honour to religious people, virtuous acts, good traditional observances, good society and conversations, reputation, gains and court-honours.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Gain of money, respect by rulers and gain of vehicles, likelihood of marriage, increase of property, illness, does many good works, acquisition of pearls or other precious stones, fatigue, addiction to immoral females and profitless discussions.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Constant sickness to family members, new enemies, some loss of property, bodily ailment, much unhappiness, displacement from home accidents, quarrels with relatives, loss of money, disease, lacking in energy, ignoble calls, mental worries, loans, danger from thieve and rulers.";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Many troubles, changes according to the position of Rahu, family disputes, journeys, fear of death, trouble from relatives and enemies, loss of peace or mental misery, loss of money, sorrows, unsuccessful in all attempts, fear of thieves and reptiles, scandals.";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Loss of money, affliction of mind with troubles, fainting or nervous exhaustion, mind full of misgivings, a long journey to a distant place, change of house due to disputes, troubles among relatives and associates, throat disease, mental anguish, ophthalmia, serious illness, fear from kings or rulers and enemies, diseases, cheated by others.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            case PlanetName.PlanetNameEnum.Moon:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Feverish complaints, pains in the eyes, success or failure according to position of the Sun and the Moon, legal power, free from diseases, decadence of enemies, happiness and prosperity, jaundice, dropsy, fever, loss of money, travels, danger to father and mother, piles, weakness, loss of children and friends.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Devoted attention to learning, love or music, good clothing, company of refined society, sound health, good reputation, journey to holy places, acquisition of abandoned wealth, power, vehicles and lands; marriage, relatives, fortunate deeds, inclination to public life, change of residence, birth of a child, increase of wealth, prosperity to relatives.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Quarrels and litigation among friends and relatives, headlong enterprises, danger of disputes between husband and wife, between lovers or in regard to marital affairs; disease, petulence, loss of money, waste of wealth, trouble from brothers and friends, danger from fever and fire, injury from instruments or stones, loss of blood and disease to household animals.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Acquisition of wealth from maternal relatives, new clothes and ornaments, settlement of disputes, pleasure through children or lover, increase of wealth, accomplishment of desires, intellectual achievements, new education, honour from rulers, general happiness, enjoyment with females, addiction to betting and drinks.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Increase of property, plenty of food and comforts, prosperous, benefits from superiors such as masters or governors, birth. of a child, vehicles, abundance of clothes and ornaments and success in undertakings, patronage or rulers, gain or property, respect, learned.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Sudden gain from wife, enjoys comforts of agriculture, water products and clothing, suffers from diseases inherited by mother, sickness, pain, loss of property, enmity, gain of houses, good works and good meals, birth of children, expenses due to marriage or other auspicious acts.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Wife's death or separation, much mental anguish, loss of property, loss of friends, ill health, mental trouble due to mother, wind and bilious affections, harsh words, and discussion with unfriendly people, disease due to indigestion, no peace of mind, quarrels with relatives.";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Distress of risks from falls and dangerous diseases, waste of wealth and loss of relatives and no ease to body, loss of money, danger of stirring up enemies, sickness, anxiety, enmity of superiors and elders, anxiety and troubles through wife, scandals, change of residence, diseases of skin, danger from thieves and poison, ill-health to father and mother, suffering from hunger.";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Illness to wife, loss of relatives, suffering from stomach ache, loss or property, sickness of a feverish nature, danger from fire, subject to swellings or eruptions, eye troubles, mind filled with cares, public criticism or displeasure, dishonor, danger to father, mother and children, scandals among equals, eating of prohibited food, bad acts, bad company, loss of money and memory.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            case PlanetName.PlanetNameEnum.Mars:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Gain of money in bad ways, destruction of enemies, good reputation, long journey to foreign lands and peace of mind, blame, odium of ciders, quarrels with them, sufferings by diseases, heartache occasioned by one's own relatives, fever or other inflammatory affection, danger of fire, troubles through persons in position, many enemies.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Profit, acquisition of wealth, house renovated or some improvements effected in it, comforts of wealth, heavy sleep, ardent passion, enjoyment by the help or women.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Great heat, dislike of friends, annoyance from brothers and sisters, danger from rulers, failure of all undertakings, danger of hurts according to the sign held by Kuja, trouble with superiors and some anxiety through strangers, foreigners or people abroad and through warlike clan. Danger of open violence, quarrel with relations, loss of money, skin disease, consumption, loss of blood, fistula, and fissures in anus, loss of females and brothers, evil doings and boils.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Marriage or inclination to marriage, knowledge and fruits of knowledge, wealth, bodily evils disappear, slander, fear of insects, poisoned by animals and insects, gain of wealth by trade, abundance of houses, trouble from enemies and mental worries, service rendered to friends and relatives, new knowledge, success in litigation.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Loss of wealth, enemies, end the unfortunate period, favour from superiors and persons in position, gain of money, birth of children, auspicious celebrations, acquisition or wealth through holy people, freedom from illness, public reputation, ascendancy and happiness.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Acquisition of property, gain of money, domestic happiness, successful love affairs, inclination towards religious observances and festivities, favourable associations, influenced by priests, skin eruptions, boils, pleasure from travelling, jewels to wife, clothing, money from relatives and brothers, odium.of females and their society, increase of intelligence, enjoyment of females and gain of money.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Loss of money, diseases, loss of relatives and danger from arms or operation, illness leading to misery, evil threatened by enemies and robbers, disputes with rulers, loss of wealth, quarrel, disputes, litigation, loss of property, cutaneous effects. loss of office or position and much anxiety.";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Danger from rulers and robbers, loss of wealth, success in evil pursuits, suffering from poisonous complaints, loss of relatives, danger from skin diseases, change of residence, some severe kind of cutaneous disease, journey to a foreign country, scandals, loss of cows and buffaloes, illness to wife, loss of memory, fear from insects and thieves, falls into well, fear from ghosts, affection of gonorrhea, fretting and";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Enmity and quarrels with low people, loss of money due to evil works, commission of signs, great sufferings due to troubles from relatives and brothers and opposition of bad people, family disputes, troubles with one's own kindred diseases, poisonous complaints, trouble through women, many enemies.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            case PlanetName.PlanetNameEnum.Mercury:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Pains in bead and stomach, enmity of people, loss of respect, danger of fire, anxieties, sickness to wire, troubles from enemies, many obstacles, troubles through superiors, acquisition of wealth.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Loss of health, some swellings or hurts in the limbs, quarrels and troubles through women, many difficulties, gain of money through ladies and agriculture and trade, success, happiness, diseases, ill-will of enemies, miscarriage of every concern, risk from quadrupeds.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Disappearance of all dangers diseases or enemies, fame derived from acts of charity and beneficence, royal favour, danger from jaundice or bilious fever, affections of the blood, neuralgic pains and headaches, troubles through neighbors, sickness, wounds or hurts, quarrels, addiction to drinks, betting and prostitutes, boils and hurts of arms, travels in forests and villages, sorrows, royal disfavour, imprisonment.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Acquisition of beautiful house and apparel, money through relatives, success in every undertaking, the birth of a brother or sister, increase in family, gain in business, good mind charitable acts, learning of mathematics and arts.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Hatred of friends, relatives and elders, wealth, liable to diseases, acquisition of land and wealth, gain by trade, reputation, good happiness, good credit, benefits from superiors, birth of a child or marriage.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Observance of duty, conformable to religion and morality, acquisition of wealth, clothes and jewels, birth or good children, happiness in married state, relatives prosper, trade increases, knowledge gained, return from a long journey, if not married, betrothal in this period, health, ornaments, vehicles, house, money gained.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Bad luck. stranger to success and happiness, severe reversal, enmity, pain in the part governed by Saturn, downfall or disgrace to relatives, mind full of evil forebodings and distress. rear from diseases, loans, loss of children, destruction of family, scandals, troubles from foreigners, earnings through evil ways, acts of charity and beneficence, acquisition of wealth, material comforts through petty chiefs, failure in agricultural operations.";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Intercourse with servants and prostitutes, skin diseases, sufferings from hot diseases, bad company and dirty meals. change or present position, fear and danger through foreigners, disputes concerning property, failure in litigation, evil dreams. headaches, sickness and loss of appetite. wealth from friends and relatives, happiness, new earnings.";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Sorrow, disease, loss of work and Dharma, bilious sickness, aimless wandering, loss of property, misfortune to relatives, troubles through doctors, mental anxiety, trouble from relatives, mental agony, loss of comfort, dread of enemy, failure in business.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            case PlanetName.PlanetNameEnum.Jupiter:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Enemies, victory, ease, great diligence, coming in of wealth, royal favour and sound health, gain,good actions or fruits of good action, loss of bodily strength.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Increase of prosperity, gain of fame and fortune, acquisition or property, benefits through children, sexual intercourse with beautiful women, good meals and clothing, success and birth or a female child or marriage to some male member in the family, gain of money.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Disappointments and troubles of various kinds, loss by thefts, loss of near and dear relatives, inflammatory disease, transfer or leave, failure in hopes and business, wandering, high fever, great risks, loss of wealth and depression of mind, pilgrimage to temples, acquisition of wealth and fame, adventures.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Increase of wealth, good and auspicious works in the house, communion with relatives, happy, increase of knowledge, acquisition of wealth through trade, favour from rulers, material comforts, perfect practice of hospitality, gain through knowledge in fine arts, birth of a well-favoured child, advantages from superiors.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Increase of property, domestic happiness, benefit from employment or occupation, birth of children, reputation, good meals, good deeds, health. royal favour, great diligence, success in all attempts, travels, dips in sacred rivers, pilgrimage, honour at stake if afflicted.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Appointment, wealth, reputation, gain of money, savings, development of sons and grandsons, jewels, good and delicious meals, marital happiness, auspicious works, reunion of the family, good success in profession or business, gain of land in the month of Taurus or Libra, much enjoyment, relatives, friends, peace or mind, acquisition or valuables, troubles from females and odium of public.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "A feeling of aversion, mental anguish, waste of wealth through sons, failure of business, increase of wealth and prosperity, pain in the body, rheumatic pains in limbs, trouble through wife or partners, failure in profit and credit, sorrows, fears, enmity of friends and relatives, adultery, unrighteous, a witness in court, quarrels in family, mental depression, funeral ceremonies for others,";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Income through low-caste people, apprehension of diseases, possibility of every possible calamity, deprivation of wealth.";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Pilgrimages to holy shrines, increase of wealth, suffering for the sake of several seniors and rulers, death of partner if in business, change of residence, separation from relatives and friends, may forsake business, poisonous effects, loss of wealth, destruction of work, illness, boils.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            case PlanetName.PlanetNameEnum.Venus:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Anxious about everything, prosperity collapses, troubles with wife, children, land, family, disputes and quarrels, diseases affecting head, belly and eyes, damage in respect of agriculture.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Gains of females, education, knowledge, money, children and vehicles, worship of God, accomplishments of desires, troubles through wife, domestic happiness afterwards, pain and disease due to inflammation of nervous tissue and from lust and other passions of human nature.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Flow of bile, disease of the eyes, great exertion, much income, acquisition or wealth, marriage, acquisition of lands, venereal diseases, danger from arms, exile in foreign places, atheistic tendencies, increase of property through the influence of females, negligence of duty, bent on pleasure and passion, temporary affection of eyes.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Association with prostitutes, enjoyments, knowledge, mathematical learning, success in litigations, inclination to learn music, piles and other hot ailments, pleasure through wife and children, increase of wealth, gain of knowledge in aru and sciences, wealth, royal favour, prosperity on a large scale and sound health.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Means of livelihood settled, gains from profession, benefits through superiors or employers or persons ruled by Jupiter fame, anxiety, quarrels with saints and religious men, gain of knowledge, end of dependence, worship of certain inferior natural forces, happiness and health, marriages, sexual intercourse, increase of family reputation and good deeds, wealth. ultimate happiness, wife and children suffer in the end.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Success, good servants and good many pleasures, money plentiful. disappearance of enemies, attainment of fame and birth of children.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Sexual intercourse with females advanced in age, accession to lands and wealth, disappearance of enemies, affection of excretory system, piles, etc., rheumatic pains in legs and bands, danger to eye sight, distaste for food, loss of appetite, physical condition poor, loss of money, wanderings, servitude, bolting and gambling, addiction to liquor, bad company, etc., ill-health, loss of memory, impotence.";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Meditation, seclusion, quarrels among relatives and his people, entire change of surroundings, schemes of deception. miserliness, acquisition of lost property, dislike of relatives. evil from friends and injury by fire.";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Discordance, death of relatives, injury inflicted by enemies, misgivings in heart, deprivation of wealth, troubles through wife, danger from quadrupeds, illness to partner or a member of the family, accidental blood poisoning, delirious fits, weakness in body and mind, gradual loss of wealth, loss of relatives, bad company, abode in seclusion, manifold sorrows, but happiness in the end.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            case PlanetName.PlanetNameEnum.Saturn:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Loss of wife and children, trouble from rulers or robbers. sinking of heart, danger of blood poisoning, haemorrhage of the generative system, chronic poisoning, intestinal swellings, affliction of the eyes, sickness even to healthy children and wife, body full of pain and disorders, danger of death. fear of death to father-in-law.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Increase in cattle, enmity of friends and relatives, cold affections, troubles and sickness, family disputes, loss of money and property, reduction to great need, mortgage of property and its recovery after a lapse of time, death of a near relative, sorrow, dislike of relatives, coming in of money, windy diseases.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Some disgrace, serious enmity, strife, much blame, wanderings from place to place, unsettled life, many enemies, loss of money by fraud or theft, change of residence, serious illness, distress to brothers and friends, hot diseases.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Charitable works, gain of wealth, birth of children, increase of knowledge in some branch, prosperity to children, success to relatives, general prosperity, favours and approbation from superiors, increase of happiness, wealth and fame, benefits occurring from acts of piety and customary religious observances, agriculture and commerce.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Worship of gods and holy people, happiness to family, increase in bodily comforts, accomplishment of intentions by the help of superiors, increase of family circles, attainment of rank.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Auspicious, general happiness, attentions and favours from others, gifts, profits in business, increase of family members, victory over enemies, success in life, goodwill of relatives, accession to wife's property and wealth.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Brings on diseases, troubles and torments, much mental anguish, capacity of kings and free-hooters, loss of wealth, fear or poisonous effect to cattle, much sufferings to family, fever, wind or phlegm, bodily ailments and colic, body languishes, loss of money and children, serious enmities, dispute and troubles from relatives, blood and bilious complaints, quarrels in family, loss of money, mental derangement.";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Disease in every limb, loss of wealth by rulers, robbers and foes, danger of physical hurts, various physical troubles, fevers, enemies, increase of troubles.";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Rheumatism or sickness, danger of poison, danger from sons, loss of money, contentions and quarrels with vile and wicked people, dread of evil dreams, quarrels in family.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            case PlanetName.PlanetNameEnum.Rahu:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Hot fevers, giddiness, fear and enmity of people, quarrels in family, benefits from persons in good position, fear and suspicion in connection with wife, children and relatives, change of position or residence, love of charitable acts, contentment, cessation of all violence and outrage of contagious diseases, success in examinations, private life happy, much reputation and fame, but mental unrest.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Abundance of enjoyments, good crops, coming in of money and communion with kith and kin, loss of relatives, loss of money through wife, pains in the limbs, change of position or residence, danger of personal hurts, unstability of health, sea voyages, gain of lands and money, loss or danger to wife and children.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Danger from rulers, fires or thieves and by arms, defeat in litigation, loss of money due to cousins, difficulties, sorrows, danger to the person due to malice of enemies, tendency to ease or dissolute habits, disputes and mental anxiety, combination of all possible calamities, bewilderment in every work and culpable failure of memory.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Many friends and relatives, wife and children, accession to wealth or royal favour. In the first 18 months of this period very busy, seriously inclined to marry. In the latter 12 months, enemies increase through his own action, happiness, birth of children, acquisition of vehicles, happiness to relatives and family, enjoyments with prostitutes, showy, gains through trade, fraudulent schemes.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Total disappearance of enemies and sickness, royal favour, acquisition of wealth, birth of children, increase of pleasure, gain through nobles or persons in power, benefits and comforts from superiors, success in all efforts, marriage in the house, increase of enemies, litigations and dips in sacred rivers.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Accession to vehicles and things of foreign land, troubles from foes, relatives and diseases, acquisition of wealth and other advantages, friendly alliances, wife a source of fortune and happiness, benefits from superiors or beads above in office, liable to deception, false friends, gain in land, birth of a child or marriage.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Scandal, danger due to fall of a tree, bad associations, divorce of wife or husband, incessant disputes and contests, rheumatism, biliousness, etc., throughout: disease due to wind and bile, distress of relatives, friends and well-wishers, residence in a remote foreign land.";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Disturbance in mind, anxieties, quarrels among relatives, death of partner, master or the head of the family, mental anxiety, danger of poisoning, transfer, all sorts of scandals and quarrels, fever, bites of insects or wounds by arms, death of relatives, going to court as witness, quarrels with parents, diseases, illness to wife, failure of intellect, loss of wealth, wandering in far-off countries and distress there.";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Danger, disease in the anus, no good and timely meals, epidemic diseases, danger of physical hurts and poison, ill-health to children, some swellings in the body, troubles through wife, danger from superiors, loss of wealth and honour, loss of children, death of cattle and misfortunes of all kinds.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            case PlanetName.PlanetNameEnum.Ketu:
                switch (minorPlanet.Name)
                {
                    case PlanetName.PlanetNameEnum.Sun:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Disappointment, physical pain, exile in foreign country, peril and obstruction in every business, increase of knowledge, sickness in family, long journey and return, anxiety about wife's health.";
                        return result;
                    case PlanetName.PlanetNameEnum.Moon:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Disputes about fair sex, trouble through children, gains and financial success, diseases of biliousness and cold, loss of relatives and money, destruction of wealth and distress of mind.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mars:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Odium of sons, wife and younger brothers, loss of relatives, trouble from diseases, foes and bad rulers, path of progress obstructed, fear and anxiety, disputes and contests of different kinds, enemies arise, danger of disputes and destruction through females, sufferings from fever, fear of robbers, death, imprisonment, urinary diseases, loss and difficulties and surgical operations.";
                        return result;
                    case PlanetName.PlanetNameEnum.Mercury:
                        result.Item1 = EventNature.Neutral;
                        result.Item2 = "Society of relatives, friends and the like, material gains from knowledge, danger from relatives, anxiety on account of children, failure in plans, deception, jealousy, falsehood, and knowledge.";
                        return result;
                    case PlanetName.PlanetNameEnum.Jupiter:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Exemption from ailments, acquisition of lands and birth of children, profitable transactions, association with people of good position, danger of poison, wife an object of pleasure, if unmarried marriage takes place.";
                        return result;
                    case PlanetName.PlanetNameEnum.Venus:
                        result.Item1 = EventNature.Good;
                        result.Item2 = "Wealth and happiness, birth of a child, efforts crowned with success, in the end sickness, wife ill, illness to children, quarrels, loss of relatives and friends, fever and dysentery.";
                        return result;
                    case PlanetName.PlanetNameEnum.Saturn:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Loss of wife, danger from enemies, imprisonment, loss of wealth, indigestion, property in danger or ruin, heavy loss in different ways, change of residence, some cutaneous diseases. anxiety owing to sickness of partner misgivings in the heart, mental anguish, difference of opinion with relations, exile in foreign countries.";
                        return result;
                    case PlanetName.PlanetNameEnum.Rahu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Loss of lands, imprisonment, quarrel with friends, danger of blood poisoning, danger of ruin, loss of property, fame and honour, rear of kings and robbers, sorrow, ruin of all business, adultery with mean women.";
                        return result;
                    case PlanetName.PlanetNameEnum.Ketu:
                        result.Item1 = EventNature.Bad;
                        result.Item2 = "Fear of death of wife or children, loss of wealth and happiness, mental troubles, separation from relatives, subject to some estrangement, restraint or detention, danger of poison.";
                        return result;
                    default:
                        throw new Exception($"Planet not accounted for! : {minorPlanet}");
                }
            default:
                throw new Exception($"Planet not accounted for! : {majorPlanet}");
        }
    }

    public static int GetCurrentDasaCountFromBirth(Time birthTime, Time currentTime)
    {
        PlanetName dasa = GetCurrentDasaBhuktiAntaram(birthTime, birthTime).Dasa;
        PlanetName dasa2 = GetCurrentDasaBhuktiAntaram(birthTime, currentTime).Dasa;
        int num = 1;
        PlanetName planetName = dasa;
        while (!(planetName == dasa2))
        {
            planetName = GetNextDasaPlanet(planetName);
            num++;
        }

        return num;
    }

    public static Dasas2 GetCurrentDasaBhuktiAntaram(Time birthTime, Time currentTime)
    {
        PlanetName constellationDasaPlanet = GetConstellationDasaPlanet(GetMoonConstellation(birthTime).GetConstellationName());
        double yearsTraversedInBirthDasa = GetYearsTraversedInBirthDasa(birthTime);
        double num = currentTime.Subtract(birthTime).TotalDays / 360.0;
        double years = yearsTraversedInBirthDasa + num;
        return GetDasaCountedFromInputDasa(constellationDasaPlanet, years);
    }

    public static Dasas2 GetDasaCountedFromInputDasa(PlanetName startDasaPlanet, double years)
    {
        double bhuktiYears = default(double);
        PlanetName dasaPlanet = GetDasa();
        double antaramYears = default(double);
        PlanetName bhuktiPlanet = GetBhukti();
        double sukshmaYears = default(double);
        PlanetName antaramPlanet = GetAntaram();
        PlanetName sukshma = GetSukshma();
        Dasas2 result = default(Dasas2);
        result.Dasa = dasaPlanet;
        result.Bhukti = bhuktiPlanet;
        result.Antaram = antaramPlanet;
        result.Sukshma = sukshma;
        return result;
        PlanetName GetAntaram()
        {
            PlanetName planetName2 = bhuktiPlanet;
            double antaramPlanetFullYears;
            while (true)
            {
                antaramPlanetFullYears = GetAntaramPlanetFullYears(dasaPlanet, bhuktiPlanet, planetName2);
                antaramYears -= antaramPlanetFullYears;
                if (antaramYears <= 0.0)
                {
                    break;
                }

                planetName2 = GetNextDasaPlanet(planetName2);
            }

            sukshmaYears = antaramYears + antaramPlanetFullYears;
            return planetName2;
        }

        PlanetName GetBhukti()
        {
            PlanetName planetName3 = dasaPlanet;
            double bhuktiPlanetFullYears;
            while (true)
            {
                bhuktiPlanetFullYears = GetBhuktiPlanetFullYears(dasaPlanet, planetName3);
                bhuktiYears -= bhuktiPlanetFullYears;
                if (bhuktiYears <= 0.0)
                {
                    break;
                }

                planetName3 = GetNextDasaPlanet(planetName3);
            }

            antaramYears = bhuktiYears + bhuktiPlanetFullYears;
            return planetName3;
        }

        PlanetName GetDasa()
        {
            PlanetName planetName4 = startDasaPlanet;
            double dasaPlanetFullYears;
            while (true)
            {
                dasaPlanetFullYears = GetDasaPlanetFullYears(planetName4);
                years -= dasaPlanetFullYears;
                if (years <= 0.0)
                {
                    break;
                }

                planetName4 = GetNextDasaPlanet(planetName4);
            }

            bhuktiYears = years + dasaPlanetFullYears;
            return planetName4;
        }

        PlanetName GetSukshma()
        {
            PlanetName planetName = antaramPlanet;
            while (true)
            {
                double sukshmaPlanetFullYears = GetSukshmaPlanetFullYears(dasaPlanet, bhuktiPlanet, antaramPlanet, planetName);
                sukshmaYears -= sukshmaPlanetFullYears;
                if (sukshmaYears <= 0.0)
                {
                    break;
                }

                planetName = GetNextDasaPlanet(planetName);
            }

            return planetName;
        }
    }

    public static PlanetName GetNextDasaPlanet(PlanetName planet)
    {
        if (planet == PlanetName.Sun)
        {
            return PlanetName.Moon;
        }

        if (planet == PlanetName.Moon)
        {
            return PlanetName.Mars;
        }

        if (planet == PlanetName.Mars)
        {
            return PlanetName.Rahu;
        }

        if (planet == PlanetName.Rahu)
        {
            return PlanetName.Jupiter;
        }

        if (planet == PlanetName.Jupiter)
        {
            return PlanetName.Saturn;
        }

        if (planet == PlanetName.Saturn)
        {
            return PlanetName.Mercury;
        }

        if (planet == PlanetName.Mercury)
        {
            return PlanetName.Ketu;
        }

        if (planet == PlanetName.Ketu)
        {
            return PlanetName.Venus;
        }

        if (planet == PlanetName.Venus)
        {
            return PlanetName.Sun;
        }

        throw new Exception("Planet not found!");
    }

    public static double GetTimeLeftInBirthDasa(Time birthTime)
    {
        double yearsTraversedInBirthDasa = GetYearsTraversedInBirthDasa(birthTime);
        PlanetName constellationDasaPlanet = GetConstellationDasaPlanet(GetMoonConstellation(birthTime).GetConstellationName());
        double dasaPlanetFullYears = GetDasaPlanetFullYears(constellationDasaPlanet);
        double num = dasaPlanetFullYears - yearsTraversedInBirthDasa;
        if (num < 0.0)
        {
            throw new Exception("Dasa years traversed is more than full years!");
        }

        return num;
    }

    public static double GetYearsTraversedInBirthDasa(Time birthTime)
    {
        PlanetConstellation moonConstellation = GetMoonConstellation(birthTime);
        double totalMinutes = moonConstellation.GetDegreesInConstellation().TotalMinutes;
        double dasaTimePerMinute = GetDasaTimePerMinute(moonConstellation.GetConstellationName());
        return totalMinutes * dasaTimePerMinute;
    }

    public static double GetDasaTimePerMinute(ConstellationName constellationName)
    {
        PlanetName constellationDasaPlanet = GetConstellationDasaPlanet(constellationName);
        double dasaPlanetFullYears = GetDasaPlanetFullYears(constellationDasaPlanet);
        return dasaPlanetFullYears / 800.0;
    }

    public static double GetDasaPlanetFullYears(PlanetName planet)
    {
        if (planet == PlanetName.Sun)
        {
            return 6.0;
        }

        if (planet == PlanetName.Moon)
        {
            return 10.0;
        }

        if (planet == PlanetName.Mars)
        {
            return 7.0;
        }

        if (planet == PlanetName.Rahu)
        {
            return 18.0;
        }

        if (planet == PlanetName.Jupiter)
        {
            return 16.0;
        }

        if (planet == PlanetName.Saturn)
        {
            return 19.0;
        }

        if (planet == PlanetName.Mercury)
        {
            return 17.0;
        }

        if (planet == PlanetName.Ketu)
        {
            return 7.0;
        }

        if (planet == PlanetName.Venus)
        {
            return 20.0;
        }

        throw new Exception("Planet not found!");
    }

    public static double GetBhuktiPlanetFullYears(PlanetName dasaPlanet, PlanetName bhuktiPlanet)
    {
        double num = GetDasaPlanetFullYears(bhuktiPlanet) / 120.0;
        return num * GetDasaPlanetFullYears(dasaPlanet);
    }

    public static double GetAntaramPlanetFullYears(PlanetName dasaPlanet, PlanetName bhuktiPlanet, PlanetName antaramPlanet)
    {
        double num = GetDasaPlanetFullYears(antaramPlanet) / 120.0;
        return num * GetBhuktiPlanetFullYears(dasaPlanet, bhuktiPlanet);
    }

    public static double GetSukshmaPlanetFullYears(PlanetName dasaPlanet, PlanetName bhuktiPlanet, PlanetName antaramPlanet, PlanetName sukshmaPlanet)
    {
        double num = GetDasaPlanetFullYears(sukshmaPlanet) / 120.0;
        return num * GetAntaramPlanetFullYears(dasaPlanet, bhuktiPlanet, antaramPlanet);
    }

    public static PlanetName GetConstellationDasaPlanet(ConstellationName constellationName)
    {
        switch (constellationName)
        {
            case ConstellationName.Krithika:
            case ConstellationName.Uttara:
            case ConstellationName.Uttarashada:
                return PlanetName.Sun;
            case ConstellationName.Rohini:
            case ConstellationName.Hasta:
            case ConstellationName.Sravana:
                return PlanetName.Moon;
            case ConstellationName.Mrigasira:
            case ConstellationName.Chitta:
            case ConstellationName.Dhanishta:
                return PlanetName.Mars;
            case ConstellationName.Aridra:
            case ConstellationName.Swathi:
            case ConstellationName.Satabhisha:
                return PlanetName.Rahu;
            case ConstellationName.Punarvasu:
            case ConstellationName.Vishhaka:
            case ConstellationName.Poorvabhadra:
                return PlanetName.Jupiter;
            case ConstellationName.Pushyami:
            case ConstellationName.Anuradha:
            case ConstellationName.Uttarabhadra:
                return PlanetName.Saturn;
            case ConstellationName.Aslesha:
            case ConstellationName.Jyesta:
            case ConstellationName.Revathi:
                return PlanetName.Mercury;
            case ConstellationName.Aswini:
            case ConstellationName.Makha:
            case ConstellationName.Moola:
                return PlanetName.Ketu;
            case ConstellationName.Bharani:
            case ConstellationName.Pubba:
            case ConstellationName.Poorvashada:
                return PlanetName.Venus;
            default:
                throw new Exception("Dasa planet for constellation not found!");
        }
    }

    public static bool IsMercuryAfflicted(Time time)
    {
        return IsMercuryMalefic(time);
    }

    public static bool IsMercuryMalefic(Time time)
    {
        if (conjunctWithMalefic())
        {
            return true;
        }

        if (conjunctWithBenefic())
        {
            return false;
        }

        return false;

        bool conjunctWithBenefic()
        {
            List<PlanetName> list = new List<PlanetName>
            {
                PlanetName.Jupiter,
                PlanetName.Venus
            };
            if (IsMoonBenefic(time))
            {
                list.Add(PlanetName.Moon);
            }

            List<PlanetName> planetsInConjuction = GetPlanetsInConjuction(time, PlanetName.Mercury);
            bool flag = false;
            foreach (PlanetName item in list)
            {
                flag = planetsInConjuction.Contains(item);
                if (flag)
                {
                    break;
                }
            }

            return flag;
        }

        bool conjunctWithMalefic()
        {
            List<PlanetName> list2 = new List<PlanetName>
            {
                PlanetName.Sun,
                PlanetName.Saturn,
                PlanetName.Mars,
                PlanetName.Rahu,
                PlanetName.Ketu
            };
            if (!IsMoonBenefic(time))
            {
                list2.Add(PlanetName.Moon);
            }

            List<PlanetName> planetsInConjuction2 = GetPlanetsInConjuction(time, PlanetName.Mercury);
            bool flag2 = false;
            foreach (PlanetName item2 in list2)
            {
                flag2 = planetsInConjuction2.Contains(item2);
                if (flag2)
                {
                    break;
                }
            }

            return flag2;
        }
    }

    public static bool IsMoonBenefic(Time time)
    {
        int lunarDateNumber = GetLunarDay(time).GetLunarDateNumber();
        if (lunarDateNumber >= 8 && lunarDateNumber <= 23)
        {
            return true;
        }

        return false;
    }

    public static bool IsPlanetBenefic(PlanetName planetName, Time time)
    {
        List<PlanetName> beneficPlanetList = GetBeneficPlanetList(time);
        return beneficPlanetList.Contains(planetName);
    }

    public static List<PlanetName> GetBeneficPlanetList(Time time)
    {
        List<PlanetName> list = new List<PlanetName>
        {
            PlanetName.Jupiter,
            PlanetName.Venus
        };
        if (IsMoonBenefic(time))
        {
            list.Add(PlanetName.Moon);
        }

        if (!IsMercuryMalefic(time))
        {
            list.Add(PlanetName.Mercury);
        }

        return list;
    }

    public static bool IsPlanetMalefic(PlanetName planetName, Time time)
    {
        List<PlanetName> maleficPlanetList = GetMaleficPlanetList(time);
        return maleficPlanetList.Contains(planetName);
    }

    public static List<PlanetName> GetMaleficPlanetList(Time time)
    {
        List<PlanetName> list = new List<PlanetName>
        {
            PlanetName.Sun,
            PlanetName.Saturn,
            PlanetName.Mars,
            PlanetName.Rahu,
            PlanetName.Ketu
        };
        if (!IsMoonBenefic(time))
        {
            list.Add(PlanetName.Moon);
        }

        if (IsMercuryMalefic(time))
        {
            list.Add(PlanetName.Mercury);
        }

        return list;
    }

    public static List<PlanetName> GetPlanetsInAspect(PlanetName planet, Time time)
    {
        List<ZodiacName> signsPlanetIsAspecting = GetSignsPlanetIsAspecting(planet, time);
        List<PlanetName> list = new List<PlanetName>();
        foreach (ZodiacName item in signsPlanetIsAspecting)
        {
            List<PlanetName> planetInSign = GetPlanetInSign(item, time);
            list.AddRange(planetInSign);
        }

        return list;
    }

    public static List<PlanetName> GetPlanetsAspectingPlanet(Time time, PlanetName receivingAspect)
    {
        return PlanetName.All9Planets.FindAll((PlanetName transmitPlanet) => IsPlanetAspectedByPlanet(receivingAspect, transmitPlanet, time));
    }

    public static List<HouseName> GetHousesInAspect(PlanetName planet, Time time)
    {
        List<ZodiacName> signsPlanetIsAspecting = GetSignsPlanetIsAspecting(planet, time);
        List<HouseName> list = new List<HouseName>();
        foreach (HouseName allHouse in House.AllHouses)
        {
            ZodiacName houseSignName = GetHouseSignName((int)allHouse, time);
            if (signsPlanetIsAspecting.Contains(houseSignName))
            {
                list.Add(allHouse);
            }
        }

        return list;
    }

    public static List<PlanetName> GetPlanetsAspectingHouse(HouseName inputHouse, Time time)
    {
        List<PlanetName> list = new List<PlanetName>();
        foreach (PlanetName all9Planet in PlanetName.All9Planets)
        {
            List<HouseName> housesInAspect = GetHousesInAspect(all9Planet, time);
            if (housesInAspect.FindAll((HouseName house) => house == inputHouse).Any())
            {
                list.Add(all9Planet);
            }
        }

        return list;
    }

    public static bool IsPlanetAspectedByPlanet(PlanetName receiveingAspect, PlanetName transmitingAspect, Time time)
    {
        List<PlanetName> planetsInAspect = GetPlanetsInAspect(transmitingAspect, time);
        return planetsInAspect.Contains(receiveingAspect);
    }

    public static bool IsHouseAspectedByPlanet(HouseName receiveingAspect, PlanetName transmitingAspect, Time time)
    {
        List<HouseName> housesInAspect = GetHousesInAspect(transmitingAspect, time);
        return housesInAspect.Contains(receiveingAspect);
    }

    public static bool IsPlanetConjunctWithPlanet(PlanetName planetA, PlanetName planetB, Time time)
    {
        List<PlanetName> planetsInConjuction = GetPlanetsInConjuction(time, planetA);
        List<PlanetName> planetsInConjuction2 = GetPlanetsInConjuction(time, planetB);
        bool result = planetsInConjuction.Contains(planetB) && planetsInConjuction2.Contains(planetA);
        if (planetsInConjuction.Contains(planetB) != planetsInConjuction2.Contains(planetA))
        {
            throw new Exception("Conjunct planet not uniform!");
        }

        return result;
    }

    public static List<PlanetName> GetAllPlanetOrderedByStrength(Time time)
    {
        Dictionary<double, PlanetName> dictionary = new Dictionary<double, PlanetName>();
        foreach (PlanetName all9Planet in PlanetName.All9Planets)
        {
            double num = GetPlanetShadbalaPinda(all9Planet, time).ToRupa();
            double key = num / getLimit(all9Planet);
            dictionary[key] = all9Planet;
        }

        List<double> list = dictionary.Keys.ToList();
        list.Sort();
        List<PlanetName> list2 = new List<PlanetName>();
        foreach (double item in list)
        {
            list2.Add(dictionary[item]);
        }

        return list2;

        static double getLimit(PlanetName _planet)
        {
            if (_planet == PlanetName.Sun)
            {
                return 5.0;
            }

            if (_planet == PlanetName.Moon)
            {
                return 6.0;
            }

            if (_planet == PlanetName.Mars)
            {
                return 5.0;
            }

            if (_planet == PlanetName.Mercury)
            {
                return 7.0;
            }

            if (_planet == PlanetName.Jupiter)
            {
                return 6.5;
            }

            if (_planet == PlanetName.Venus)
            {
                return 5.5;
            }

            if (!(_planet == PlanetName.Saturn))
            {
                throw new Exception("Planet not specified!");
            }

            return 5.0;
        }
    }

    public static List<(double, PlanetName)> GetAllPlanetStrength(Time time)
    {
        List<(double, PlanetName)> list = new List<(double, PlanetName)>();
        foreach (PlanetName all9Planet in PlanetName.All9Planets)
        {
            double item = GetPlanetShadbalaPinda(all9Planet, time).ToDouble();
            list.Add((item, all9Planet));
        }

        return list;
    }

    public static HouseName[] GetAllHousesOrderedByStrength(Time time)
    {
        Dictionary<double, HouseName> dictionary = new Dictionary<double, HouseName>();
        foreach (HouseName allHouse in House.AllHouses)
        {
            double key = GetBhavabala(allHouse, time).ToRupa();
            dictionary[key] = allHouse;
        }

        List<double> list = dictionary.Keys.ToList();
        list.Sort();
        HouseName[] array = new HouseName[12];
        int num = 11;
        foreach (double item in list)
        {
            array[num] = dictionary[item];
            num--;
        }

        return array;
    }

    public static Shashtiamsa GetPlanetShadbalaPinda(PlanetName planetName, Time time)
    {
        if (planetName == PlanetName.Rahu || planetName == PlanetName.Ketu)
        {
            PlanetName lordOfHousePlanetIsIn = GetLordOfHousePlanetIsIn(time, planetName);
            planetName = lordOfHousePlanetIsIn;
        }

        Shashtiamsa planetSthanaBala = GetPlanetSthanaBala(planetName, time);
        Shashtiamsa planetDigBala = GetPlanetDigBala(planetName, time);
        Shashtiamsa planetKalaBala = GetPlanetKalaBala(planetName, time);
        Shashtiamsa planetChestaBala = GetPlanetChestaBala(planetName, time);
        Shashtiamsa planetNaisargikaBala = GetPlanetNaisargikaBala(planetName, time);
        Shashtiamsa planetDrikBala = GetPlanetDrikBala(planetName, time);
        return new Shashtiamsa(Math.Round((planetSthanaBala + planetDigBala + planetKalaBala + planetChestaBala + planetNaisargikaBala + planetDrikBala).ToDouble(), 2));
    }

    private static PlanetName GetLordOfHousePlanetIsIn(Time time, PlanetName planetName)
    {
        int housePlanetIsIn = GetHousePlanetIsIn(time, planetName);
        return GetLordOfHouse((HouseName)housePlanetIsIn, time);
    }

    public static Shashtiamsa GetPlanetDrikBala(PlanetName planetName, Time time)
    {
        Dictionary<string, double> dictionary = new Dictionary<string, double>();
        Dictionary<PlanetName, int> dictionary2 = new Dictionary<PlanetName, int>();
        foreach (PlanetName all7Planet in PlanetName.All7Planets)
        {
            if (IsPlanetBenefic(all7Planet, time))
            {
                dictionary2[all7Planet] = 1;
            }
            else
            {
                dictionary2[all7Planet] = -1;
            }
        }

        foreach (PlanetName all7Planet2 in PlanetName.All7Planets)
        {
            foreach (PlanetName all7Planet3 in PlanetName.All7Planets)
            {
                double totalDegrees = GetPlanetNirayanaLongitude(time, all7Planet3).TotalDegrees;
                double totalDegrees2 = GetPlanetNirayanaLongitude(time, all7Planet2).TotalDegrees;
                double num = totalDegrees - totalDegrees2;
                if (num < 0.0)
                {
                    num += 360.0;
                }

                double num2 = FindViseshaDrishti(num, all7Planet2);
                dictionary[all7Planet2.ToString() + all7Planet3.ToString()] = FindDrishtiValue(num) + num2;
            }
        }

        double num3 = 0.0;
        Dictionary<PlanetName, double> dictionary3 = new Dictionary<PlanetName, double>();
        foreach (PlanetName all7Planet4 in PlanetName.All7Planets)
        {
            num3 = 0.0;
            foreach (PlanetName all7Planet5 in PlanetName.All7Planets)
            {
                num3 += (double)dictionary2[all7Planet5] * dictionary[all7Planet5.ToString() + all7Planet4.ToString()];
            }

            dictionary3[all7Planet4] = num3 / 4.0;
        }

        return new Shashtiamsa(dictionary3[planetName]);
    }

    public static double FindViseshaDrishti(double dk, PlanetName p)
    {
        double result = 0.0;
        if (p == PlanetName.Saturn)
        {
            if ((dk >= 60.0 && dk <= 90.0) || (dk >= 270.0 && dk <= 300.0))
            {
                result = 45.0;
            }
        }
        else if (p == PlanetName.Jupiter)
        {
            if ((dk >= 120.0 && dk <= 150.0) || (dk >= 240.0 && dk <= 270.0))
            {
                result = 30.0;
            }
        }
        else if (p == PlanetName.Mars)
        {
            if ((dk >= 90.0 && dk <= 120.0) || (dk >= 210.0 && dk <= 240.0))
            {
                result = 15.0;
            }
        }
        else
        {
            result = 0.0;
        }

        return result;
    }

    public static double FindDrishtiValue(double dk)
    {
        double result = 0.0;
        if (dk >= 30.0 && dk <= 60.0)
        {
            result = (dk - 30.0) / 2.0;
        }
        else if (dk > 60.0 && dk <= 90.0)
        {
            result = dk - 60.0 + 15.0;
        }
        else if (dk > 90.0 && dk <= 120.0)
        {
            result = (120.0 - dk) / 2.0 + 30.0;
        }
        else if (dk > 120.0 && dk <= 150.0)
        {
            result = 150.0 - dk;
        }
        else if (dk > 150.0 && dk <= 180.0)
        {
            result = (dk - 150.0) * 2.0;
        }
        else if (dk > 180.0 && dk <= 300.0)
        {
            result = (300.0 - dk) / 2.0;
        }

        return result;
    }

    public static Shashtiamsa GetPlanetNaisargikaBala(PlanetName planetName, Time time)
    {
        if (planetName == PlanetName.Sun)
        {
            return new Shashtiamsa(60.0);
        }

        if (planetName == PlanetName.Moon)
        {
            return new Shashtiamsa(51.43);
        }

        if (planetName == PlanetName.Venus)
        {
            return new Shashtiamsa(42.85);
        }

        if (planetName == PlanetName.Jupiter)
        {
            return new Shashtiamsa(34.28);
        }

        if (planetName == PlanetName.Mercury)
        {
            return new Shashtiamsa(25.7);
        }

        if (planetName == PlanetName.Mars)
        {
            return new Shashtiamsa(17.14);
        }

        if (planetName == PlanetName.Saturn)
        {
            return new Shashtiamsa(8.57);
        }

        throw new Exception("Planet not specified!");
    }

    public static Shashtiamsa GetPlanetChestaBala(PlanetName planetName, Time time)
    {
        if (planetName == PlanetName.Sun || planetName == PlanetName.Moon || planetName == PlanetName.Rahu || planetName == PlanetName.Ketu)
        {
            return Shashtiamsa.Zero;
        }

        double epochInterval = GetEpochInterval(time);
        Dictionary<PlanetName, double> madhya = GetMadhya(epochInterval, time);
        Dictionary<PlanetName, double> dictionary = GetSeeghrochcha(madhya, epochInterval, time);
        double totalDegrees = GetPlanetNirayanaLongitude(time, planetName).TotalDegrees;
        double num = dictionary[planetName];
        double num2 = madhya[planetName];
        double num3 = (num2 + totalDegrees) / 2.0;
        double num4 = num - num3;
        if (num4 < 360.0)
        {
            num4 += 360.0;
        }

        num4 %= 360.0;
        if (num4 > 180.0)
        {
            num4 = 360.0 - num4;
        }

        double shashtiamsa = num4 / 3.0;
        return new Shashtiamsa(shashtiamsa);
        static Dictionary<PlanetName, double> GetSeeghrochcha(Dictionary<PlanetName, double> mean, double epochToBirthDays, Time time1)
        {
            int year = time1.GetLmtDateTimeOffset().Year;
            Dictionary<PlanetName, double> dictionary2 = new Dictionary<PlanetName, double>();
            PlanetName mars = PlanetName.Mars;
            PlanetName jupiter = PlanetName.Jupiter;
            double num6 = (dictionary2[PlanetName.Saturn] = mean[PlanetName.Sun]);
            double value = (dictionary2[jupiter] = num6);
            dictionary2[mars] = value;
            double num8 = 6.67 + 0.00133 * (double)(year - 1900);
            double num9 = epochToBirthDays * 4.092385;
            double num10 = ((num9 < 0.0) ? (164.0 - num9) : (164.0 + num9));
            num10 -= num8;
            dictionary2[PlanetName.Mercury] = (num10 + num8) % 360.0;
            num8 = 5.0 + 0.0001 * (double)(year - 1900);
            double num11 = epochToBirthDays * 1.602159;
            double num12 = ((num11 < 0.0) ? (328.51 - num11) : (328.51 + num11));
            num12 -= num8;
            dictionary2[PlanetName.Venus] = num12 % 360.0;
            return dictionary2;
        }
    }

    public static Dictionary<PlanetName, double> GetMadhya(double epochToBirthDays, Time time1)
    {
        int year = time1.GetLmtDateTimeOffset().Year;
        Dictionary<PlanetName, double> dictionary = new Dictionary<PlanetName, double>();
        double num = 257.4568;
        double num2 = epochToBirthDays * 0.9855931;
        double num3 = ((num2 < 0.0) ? (num - num2) : (num + num2));
        num3 %= 360.0;
        dictionary[PlanetName.Sun] = num3;
        PlanetName mercury = PlanetName.Mercury;
        double value = (dictionary[PlanetName.Venus] = dictionary[PlanetName.Sun]);
        dictionary[mercury] = value;
        double num5 = 270.22;
        double num6 = epochToBirthDays * 0.5240218;
        double num7 = ((num6 < 0.0) ? (num5 - num6) : (num5 + num6));
        num7 %= 360.0;
        dictionary[PlanetName.Mars] = num7;
        double num8 = 220.04;
        double num9 = epochToBirthDays * 0.08310024;
        double num10 = ((num9 < 0.0) ? (num8 - num9) : (num8 + num9));
        double num11 = 3.33 + 0.0067 * (double)(year - 1900);
        num10 -= num11;
        num10 %= 360.0;
        dictionary[PlanetName.Jupiter] = num10;
        double num12 = 220.04;
        double num13 = epochToBirthDays * 0.03333857;
        double num14 = ((num13 < 0.0) ? (num12 - num13) : (num12 + num13));
        double num15 = 5.0 + 0.001 * (double)(year - 1900);
        num14 += num15;
        num14 %= 360.0;
        dictionary[PlanetName.Saturn] = num14;
        if (dictionary.Any((KeyValuePair<PlanetName, double> x) => x.Value < 0.0))
        {
            throw new Exception("Madya/Mean can't be negative!");
        }

        return dictionary;
    }

    public static double GetEpochInterval(Time time1)
    {
        int year = time1.GetLmtDateTimeOffset().Year;
        int month = time1.GetLmtDateTimeOffset().Month;
        int day = time1.GetLmtDateTimeOffset().Day;
        int[] array = new int[13]
        {
            0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
            304, 334, 365
        };
        int num = year - 1900;
        int num2 = num * 365 + num / 4 + array[month - 1] - 1 + day;
        int hour = time1.GetLmtDateTimeOffset().Hour;
        int minute = time1.GetLmtDateTimeOffset().Minute;
        double totalHours = time1.GetLmtDateTimeOffset().Offset.TotalHours;
        double num3 = new TimeSpan(hour, minute, 0).TotalHours + (5.0666666666666664 - totalHours);
        double value = (double)num2 + num3 / 24.0;
        return Math.Round(value, 3);
    }

    public static PlanetMotion GetPlanetMotionName(PlanetName planetName, Time time)
    {
        if (planetName == PlanetName.Sun || planetName == PlanetName.Moon || planetName == PlanetName.Rahu || planetName == PlanetName.Ketu)
        {
            return PlanetMotion.Direct;
        }

        double num = GetPlanetChestaBala(planetName, time).ToDouble();
        double num2 = num;
        double num3 = num2;
        if (num3 <= 60.0)
        {
            if (num3 > 45.0)
            {
                return PlanetMotion.Retrograde;
            }

            if (num3 > 15.0)
            {
                return PlanetMotion.Direct;
            }

            if (num3 >= 0.0)
            {
                return PlanetMotion.Stationary;
            }
        }

        throw new Exception($"Error in GetPlanetMotionName : {num}");
    }

    public static double GetPlanetCirculationTime(PlanetName planetName)
    {
        if (planetName == PlanetName.Sun)
        {
            return 1.0;
        }

        if (planetName == PlanetName.Moon)
        {
            return 0.082;
        }

        if (planetName == PlanetName.Mars)
        {
            return 1.88;
        }

        if (planetName == PlanetName.Mercury)
        {
            return 0.24;
        }

        if (planetName == PlanetName.Jupiter)
        {
            return 11.86;
        }

        if (planetName == PlanetName.Venus)
        {
            return 0.62;
        }

        if (planetName == PlanetName.Saturn)
        {
            return 29.46;
        }

        throw new Exception("Planet circulation time not defined!");
    }

    public static Shashtiamsa GetPlanetSaptavargajaBala(PlanetName planetName, Time time)
    {
        double num = 0.0;
        List<PlanetToSignRelationship> list = new List<PlanetToSignRelationship>();
        if (IsPlanetInMoolatrikona(planetName, time))
        {
            list.Add(PlanetToSignRelationship.Moolatrikona);
        }
        else
        {
            ZodiacName signName = GetPlanetRasiSign(planetName, time).GetSignName();
            PlanetToSignRelationship planetRelationshipWithSign = GetPlanetRelationshipWithSign(planetName, signName, time);
            list.Add(planetRelationshipWithSign);
        }

        ZodiacName planetHoraSign = GetPlanetHoraSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign2 = GetPlanetRelationshipWithSign(planetName, planetHoraSign, time);
        list.Add(planetRelationshipWithSign2);
        ZodiacName planetDrekkanaSign = GetPlanetDrekkanaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign3 = GetPlanetRelationshipWithSign(planetName, planetDrekkanaSign, time);
        list.Add(planetRelationshipWithSign3);
        ZodiacName planetSaptamsaSign = GetPlanetSaptamsaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign4 = GetPlanetRelationshipWithSign(planetName, planetSaptamsaSign, time);
        list.Add(planetRelationshipWithSign4);
        ZodiacName planetNavamsaSign = GetPlanetNavamsaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign5 = GetPlanetRelationshipWithSign(planetName, planetNavamsaSign, time);
        list.Add(planetRelationshipWithSign5);
        ZodiacName planetDwadasamsaSign = GetPlanetDwadasamsaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign6 = GetPlanetRelationshipWithSign(planetName, planetDwadasamsaSign, time);
        list.Add(planetRelationshipWithSign6);
        ZodiacName planetThrimsamsaSign = GetPlanetThrimsamsaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign7 = GetPlanetRelationshipWithSign(planetName, planetThrimsamsaSign, time);
        list.Add(planetRelationshipWithSign7);
        foreach (PlanetToSignRelationship item in list)
        {
            if (item == PlanetToSignRelationship.Moolatrikona)
            {
                num += 45.0;
            }

            if (item == PlanetToSignRelationship.OwnVarga)
            {
                num += 30.0;
            }

            if (item == PlanetToSignRelationship.BestFriendVarga)
            {
                num += 22.5;
            }

            if (item == PlanetToSignRelationship.FriendVarga)
            {
                num += 15.0;
            }

            if (item == PlanetToSignRelationship.NeutralVarga)
            {
                num += 7.5;
            }

            if (item == PlanetToSignRelationship.EnemyVarga)
            {
                num += 3.75;
            }

            if (item == PlanetToSignRelationship.BitterEnemyVarga)
            {
                num += 1.875;
            }
        }

        return new Shashtiamsa(num);
    }

    public static Shashtiamsa GetPlanetShadvargaBala(PlanetName planetName, Time time)
    {
        List<PlanetToSignRelationship> list = new List<PlanetToSignRelationship>();
        if (IsPlanetInMoolatrikona(planetName, time))
        {
            list.Add(PlanetToSignRelationship.Moolatrikona);
        }
        else
        {
            ZodiacName signName = GetPlanetRasiSign(planetName, time).GetSignName();
            PlanetToSignRelationship planetRelationshipWithSign = GetPlanetRelationshipWithSign(planetName, signName, time);
            list.Add(planetRelationshipWithSign);
        }

        ZodiacName planetHoraSign = GetPlanetHoraSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign2 = GetPlanetRelationshipWithSign(planetName, planetHoraSign, time);
        list.Add(planetRelationshipWithSign2);
        ZodiacName planetDrekkanaSign = GetPlanetDrekkanaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign3 = GetPlanetRelationshipWithSign(planetName, planetDrekkanaSign, time);
        list.Add(planetRelationshipWithSign3);
        ZodiacName planetNavamsaSign = GetPlanetNavamsaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign4 = GetPlanetRelationshipWithSign(planetName, planetNavamsaSign, time);
        list.Add(planetRelationshipWithSign4);
        ZodiacName planetDwadasamsaSign = GetPlanetDwadasamsaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign5 = GetPlanetRelationshipWithSign(planetName, planetDwadasamsaSign, time);
        list.Add(planetRelationshipWithSign5);
        ZodiacName planetThrimsamsaSign = GetPlanetThrimsamsaSign(planetName, time);
        PlanetToSignRelationship planetRelationshipWithSign6 = GetPlanetRelationshipWithSign(planetName, planetThrimsamsaSign, time);
        list.Add(planetRelationshipWithSign6);
        double num = 0.0;
        foreach (PlanetToSignRelationship item in list)
        {
            if (item == PlanetToSignRelationship.Moolatrikona)
            {
                num += 45.0;
            }

            if (item == PlanetToSignRelationship.OwnVarga)
            {
                num += 30.0;
            }

            if (item == PlanetToSignRelationship.BestFriendVarga)
            {
                num += 22.5;
            }

            if (item == PlanetToSignRelationship.FriendVarga)
            {
                num += 15.0;
            }

            if (item == PlanetToSignRelationship.NeutralVarga)
            {
                num += 7.5;
            }

            if (item == PlanetToSignRelationship.EnemyVarga)
            {
                num += 3.75;
            }

            if (item == PlanetToSignRelationship.BitterEnemyVarga)
            {
                num += 1.875;
            }
        }

        return new Shashtiamsa(num);
    }

    public static bool IsPlanetStrongInShadvarga(PlanetName planet, Time time)
    {
        double num = GetPlanetShadvargaBala(planet, time).ToDouble();
        double planetShadvargaBalaNeutralPoint = GetPlanetShadvargaBalaNeutralPoint(planet);
        return num > 140.0;
    }

    public static Shashtiamsa GetPlanetSthanaBala(PlanetName planetName, Time time)
    {
        Shashtiamsa planetOchchabala = GetPlanetOchchabala(planetName, time);
        Shashtiamsa planetSaptavargajaBala = GetPlanetSaptavargajaBala(planetName, time);
        Shashtiamsa planetOjayugmarasyamsaBala = GetPlanetOjayugmarasyamsaBala(planetName, time);
        Shashtiamsa planetKendraBala = GetPlanetKendraBala(planetName, time);
        Shashtiamsa planetDrekkanaBala = GetPlanetDrekkanaBala(planetName, time);
        return planetOchchabala + planetSaptavargajaBala + planetOjayugmarasyamsaBala + planetKendraBala + planetDrekkanaBala;
    }

    public static Shashtiamsa GetPlanetDrekkanaBala(PlanetName planetName, Time time)
    {
        double totalDegrees = GetPlanetRasiSign(planetName, time).GetDegreesInSign().TotalDegrees;
        if ((planetName == PlanetName.Sun || planetName == PlanetName.Jupiter || planetName == PlanetName.Mars) && totalDegrees >= 0.0 && totalDegrees <= 10.0)
        {
            return new Shashtiamsa(15.0);
        }

        if ((planetName == PlanetName.Saturn || planetName == PlanetName.Mercury) && totalDegrees > 10.0 && totalDegrees <= 20.0)
        {
            return new Shashtiamsa(15.0);
        }

        if ((planetName == PlanetName.Moon || planetName == PlanetName.Venus) && totalDegrees > 20.0 && totalDegrees <= 30.0)
        {
            return new Shashtiamsa(15.0);
        }

        return new Shashtiamsa(0.0);
    }

    public static Shashtiamsa GetPlanetKendraBala(PlanetName planetName, Time time)
    {
        int signName = (int)GetPlanetRasiSign(planetName, time).GetSignName();
        if (signName == 1 || signName == 4 || signName == 7 || signName == 10)
        {
            return new Shashtiamsa(60.0);
        }

        if (signName == 2 || signName == 5 || signName == 8 || signName == 11)
        {
            return new Shashtiamsa(30.0);
        }

        if (signName == 3 || signName == 6 || signName == 9 || signName == 12)
        {
            return new Shashtiamsa(15.0);
        }

        throw new Exception("Kendra Bala not found, error");
    }

    public static Shashtiamsa GetPlanetOjayugmarasyamsaBala(PlanetName planetName, Time time)
    {
        ZodiacName signName = GetPlanetRasiSign(planetName, time).GetSignName();
        ZodiacName planetNavamsaSign = GetPlanetNavamsaSign(planetName, time);
        double num = 0.0;
        if (planetName == PlanetName.Moon || planetName == PlanetName.Venus)
        {
            if (IsEvenSign(signName))
            {
                num += 15.0;
            }

            if (IsEvenSign(planetNavamsaSign))
            {
                num += 15.0;
            }
        }
        else if (planetName == PlanetName.Sun || planetName == PlanetName.Mars || planetName == PlanetName.Jupiter || planetName == PlanetName.Mercury || planetName == PlanetName.Saturn)
        {
            if (IsOddSign(signName))
            {
                num += 15.0;
            }

            if (IsOddSign(planetNavamsaSign))
            {
                num += 15.0;
            }
        }

        return new Shashtiamsa(num);
    }

    public static Shashtiamsa GetPlanetKalaBala(PlanetName planetName, Time time)
    {
        Dictionary<PlanetName, Shashtiamsa> dictionary = new Dictionary<PlanetName, Shashtiamsa>();
        foreach (PlanetName all7Planet in PlanetName.All7Planets)
        {
            Shashtiamsa preKalaBala = GetPreKalaBala(all7Planet, time);
            dictionary.Add(all7Planet, preKalaBala);
        }

        Shashtiamsa planetYuddhaBala = GetPlanetYuddhaBala(planetName, dictionary, time);
        return dictionary[planetName] + planetYuddhaBala;

        static Shashtiamsa GetPreKalaBala(PlanetName planetName, Time time)
        {
            Shashtiamsa planetNathonnathaBala = GetPlanetNathonnathaBala(planetName, time);
            Shashtiamsa planetPakshaBala = GetPlanetPakshaBala(planetName, time);
            Shashtiamsa planetTribhagaBala = GetPlanetTribhagaBala(planetName, time);
            Shashtiamsa planetAbdaBala = GetPlanetAbdaBala(planetName, time);
            Shashtiamsa planetMasaBala = GetPlanetMasaBala(planetName, time);
            Shashtiamsa planetVaraBala = GetPlanetVaraBala(planetName, time);
            Shashtiamsa planetHoraBala = GetPlanetHoraBala(planetName, time);
            Shashtiamsa planetAyanaBala = GetPlanetAyanaBala(planetName, time);
            return planetNathonnathaBala + planetPakshaBala + planetTribhagaBala + planetAbdaBala + planetMasaBala + planetVaraBala + planetHoraBala + planetAyanaBala;
        }
    }

    public static Shashtiamsa GetPlanetYuddhaBala(PlanetName inputedPlanet, Dictionary<PlanetName, Shashtiamsa> preKalaBalaValues, Time time)
    {
        if (inputedPlanet == PlanetName.Moon || inputedPlanet == PlanetName.Sun)
        {
            return Shashtiamsa.Zero;
        }

        Dictionary<PlanetName, Shashtiamsa> dictionary = new Dictionary<PlanetName, Shashtiamsa>();
        List<PlanetName> planetsInConjuction = GetPlanetsInConjuction(time, inputedPlanet);
        planetsInConjuction.RemoveAll((PlanetName pl) => pl == PlanetName.Rahu || pl == PlanetName.Ketu);
        foreach (PlanetName item in planetsInConjuction)
        {
            if (item == PlanetName.Moon || item == PlanetName.Sun)
            {
                continue;
            }

            Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, inputedPlanet);
            Angle planetNirayanaLongitude2 = GetPlanetNirayanaLongitude(time, item);
            Angle distanceBetweenPlanets = GetDistanceBetweenPlanets(planetNirayanaLongitude, planetNirayanaLongitude2);
            if (distanceBetweenPlanets < Angle.FromDegrees(1.0))
            {
                PlanetName key = null;
                PlanetName key2 = null;
                if (planetNirayanaLongitude < planetNirayanaLongitude2)
                {
                    key = inputedPlanet;
                    key2 = item;
                }
                else if (planetNirayanaLongitude > planetNirayanaLongitude2)
                {
                    key = item;
                    key2 = inputedPlanet;
                }
                else if (planetNirayanaLongitude == planetNirayanaLongitude2)
                {
                    LogManager.Error("Planets same longitude! Not expected, random result used!");
                    key = inputedPlanet;
                    key2 = item;
                }

                double num = Math.Abs(preKalaBalaValues[inputedPlanet].ToDouble() - preKalaBalaValues[item].ToDouble());
                Angle difference = GetPlanetDiscDiameter(inputedPlanet).GetDifference(GetPlanetDiscDiameter(item));
                double num2 = difference.TotalDegrees / num;
                dictionary[key] = new Shashtiamsa(num2);
                dictionary[key2] = new Shashtiamsa(0.0 - num2);
            }
        }

        Shashtiamsa value;
        return dictionary.TryGetValue(inputedPlanet, out value) ? value : Shashtiamsa.Zero;
        static Angle GetPlanetDiscDiameter(PlanetName planet)
        {
            if (planet == PlanetName.Mars)
            {
                return new Angle(0.0, 9.0, 4L);
            }

            if (planet == PlanetName.Mercury)
            {
                return new Angle(0.0, 6.0, 6L);
            }

            if (planet == PlanetName.Jupiter)
            {
                return new Angle(0.0, 190.0, 4L);
            }

            if (planet == PlanetName.Venus)
            {
                return new Angle(0.0, 16.0, 6L);
            }

            if (!(planet == PlanetName.Saturn))
            {
                throw new Exception("Disc diameter now found!");
            }

            return new Angle(0.0, 158.0, 0L);
        }
    }

    public static Shashtiamsa GetPlanetAyanaBala(PlanetName planetName, Time time)
    {
        double num = 0.0;
        double planetDeclination = GetPlanetDeclination(planetName, time);
        Angle angle = Angle.FromDegrees(24.0);
        bool flag = !(planetDeclination < 0.0);
        double num2 = Math.Abs(planetDeclination);
        if (planetName == PlanetName.Venus || planetName == PlanetName.Sun || planetName == PlanetName.Mars || planetName == PlanetName.Jupiter)
        {
            num = ((!flag) ? ((24.0 - num2) / 48.0 * 60.0) : ((24.0 + num2) / 48.0 * 60.0));
            if (planetName == PlanetName.Sun)
            {
                num *= 2.0;
            }
        }
        else if (planetName == PlanetName.Saturn || planetName == PlanetName.Moon)
        {
            num = (flag ? ((24.0 - num2) / 48.0 * 60.0) : ((24.0 + num2) / 48.0 * 60.0));
        }
        else if (planetName == PlanetName.Mercury)
        {
            num = (24.0 + num2) / 48.0 * 60.0;
        }

        return new Shashtiamsa(num);
    }

    public static double GetPlanetDeclination(PlanetName planetName, Time time)
    {
        double planetEps = GetPlanetEps(planetName, time);
        Angle planetSayanaLongitude = GetPlanetSayanaLongitude(time, planetName);
        Angle planetSayanaLatitude = GetPlanetSayanaLatitude(time, planetName);
        return planetSayanaLatitude.TotalDegrees + planetEps * Math.Sin(0.0174532925199433 * planetSayanaLongitude.TotalDegrees);
    }

    public static double GetPlanetEps(PlanetName planetName, Time time)
    {
        string serr = "";
        double[] array = new double[6];
        SwissEph swissEph = new SwissEph();
        double tjd = TimeToEphemerisTime(time);
        swissEph.swe_calc(tjd, -1, 0, array, ref serr);
        return array[0];
    }

    public static Shashtiamsa GetPlanetHoraBala(PlanetName planetName, Time time)
    {
        DayOfWeek dayOfWeek = GetDayOfWeek(time);
        int horaAtBirth = GetHoraAtBirth(time);
        PlanetName lordOfHora = GetLordOfHora(horaAtBirth, dayOfWeek);
        if (lordOfHora == planetName)
        {
            return new Shashtiamsa(60.0);
        }

        return Shashtiamsa.Zero;
    }

    public static Shashtiamsa GetPlanetAbdaBala(PlanetName planetName, Time time)
    {
        dynamic yearAndMonthLord = GetYearAndMonthLord(time);
        PlanetName planetName2 = yearAndMonthLord.YearLord;
        if (planetName2 == planetName)
        {
            return new Shashtiamsa(15.0);
        }

        return Shashtiamsa.Zero;
    }

    public static Shashtiamsa GetPlanetMasaBala(PlanetName planetName, Time time)
    {
        dynamic yearAndMonthLord = GetYearAndMonthLord(time);
        PlanetName planetName2 = yearAndMonthLord.MonthLord;
        if (planetName2 == planetName)
        {
            return new Shashtiamsa(30.0);
        }

        return Shashtiamsa.Zero;
    }

    public static Shashtiamsa GetPlanetVaraBala(PlanetName planetName, Time time)
    {
        PlanetName lordOfWeekday = GetLordOfWeekday(time);
        if (lordOfWeekday == planetName)
        {
            return new Shashtiamsa(45.0);
        }

        return Shashtiamsa.Zero;
    }

    public static object GetYearAndMonthLord(Time time)
    {
        PlanetName sun = PlanetName.Sun;
        PlanetName sun2 = PlanetName.Sun;
        using SwissEph swissEph = new SwissEph();
        double num = swissEph.swe_julday(1827, 5, 2, 0.0, 1);
        double greenwichLmtInJulianDays = GetGreenwichLmtInJulianDays(time);
        double num2 = greenwichLmtInJulianDays - num;
        if (num2 >= 0.0)
        {
            double num3 = Math.Floor(num2 / 360.0);
            num2 -= num3 * 360.0;
        }
        else
        {
            double num4 = 0.0 - num2;
            double num5 = Math.Ceiling(num4 / 360.0);
            num2 += num5 * 360.0;
        }

        double num6 = num2;
        while (num2 > 30.0)
        {
            num2 -= 30.0;
        }

        double num7 = num2;
        while (num2 > 7.0)
        {
            num2 -= 7.0;
        }

        int dayNumber2 = swissEph.swe_day_of_week(greenwichLmtInJulianDays - num6);
        int dayNumber3 = swissEph.swe_day_of_week(greenwichLmtInJulianDays - num7);
        DayOfWeek weekday = swissEphWeekDayToMuhurthaDay(dayNumber2);
        DayOfWeek weekday2 = swissEphWeekDayToMuhurthaDay(dayNumber3);
        sun = GetLordOfWeekday(weekday);
        sun2 = GetLordOfWeekday(weekday2);
        return new
        {
            YearLord = sun,
            MonthLord = sun2
        };

        static DayOfWeek swissEphWeekDayToMuhurthaDay(int dayNumber)
        {
            return dayNumber switch
            {
                0 => DayOfWeek.Monday,
                1 => DayOfWeek.Tuesday,
                2 => DayOfWeek.Wednesday,
                3 => DayOfWeek.Thursday,
                4 => DayOfWeek.Friday,
                5 => DayOfWeek.Saturday,
                6 => DayOfWeek.Sunday,
                _ => throw new Exception("Invalid day number!"),
            };
        }
    }

    public static Shashtiamsa GetPlanetTribhagaBala(PlanetName planetName, Time time)
    {
        PlanetName planetName2 = PlanetName.Jupiter;
        Time sunsetTime = GetSunsetTime(time);
        if (IsDayBirth(time))
        {
            Time sunriseTime = GetSunriseTime(time);
            double num = sunsetTime.Subtract(sunriseTime).TotalHours / 3.0;
            double totalHours = time.Subtract(sunriseTime).TotalHours;
            switch ((int)Math.Floor(totalHours / num))
            {
                case 0:
                    planetName2 = PlanetName.Mercury;
                    break;
                case 1:
                    planetName2 = PlanetName.Sun;
                    break;
                case 2:
                    planetName2 = PlanetName.Saturn;
                    break;
            }
        }
        else
        {
            Time time2 = time.AddHours(24.0);
            double num2 = GetSunriseTime(time2).Subtract(sunsetTime).TotalHours / 3.0;
            double totalHours2 = time.Subtract(sunsetTime).TotalHours;
            switch ((int)Math.Floor(totalHours2 / num2))
            {
                case 0:
                    planetName2 = PlanetName.Moon;
                    break;
                case 1:
                    planetName2 = PlanetName.Venus;
                    break;
                case 2:
                    planetName2 = PlanetName.Mars;
                    break;
            }
        }

        if (planetName == PlanetName.Jupiter || planetName == planetName2)
        {
            return new Shashtiamsa(60.0);
        }

        return new Shashtiamsa(0.0);
    }

    public static Shashtiamsa GetPlanetOchchabala(PlanetName planetName, Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planetName);
        ZodiacSign planetDebilitationPoint = GetPlanetDebilitationPoint(planetName);
        Angle longitudeAtZodiacSign = GetLongitudeAtZodiacSign(planetDebilitationPoint);
        Angle distanceBetweenPlanets = GetDistanceBetweenPlanets(planetNirayanaLongitude, longitudeAtZodiacSign);
        if (distanceBetweenPlanets.TotalDegrees > 180.0)
        {
            distanceBetweenPlanets = GetDistanceBetweenPlanets(distanceBetweenPlanets, Angle.Degrees360);
        }

        double shashtiamsa = distanceBetweenPlanets.TotalDegrees / 3.0;
        return new Shashtiamsa(shashtiamsa);
    }

    public static bool IsDayBirth(Time time)
    {
        DateTimeOffset lmtDateTimeOffset = GetSunriseTime(time).GetLmtDateTimeOffset();
        DateTimeOffset lmtDateTimeOffset2 = GetSunsetTime(time).GetLmtDateTimeOffset();
        DateTimeOffset lmtDateTimeOffset3 = time.GetLmtDateTimeOffset();
        if (lmtDateTimeOffset3 >= lmtDateTimeOffset && lmtDateTimeOffset3 <= lmtDateTimeOffset2)
        {
            return true;
        }

        return false;
    }

    public static Shashtiamsa GetPlanetPakshaBala(PlanetName planetName, Time time)
    {
        double num = 0.0;
        MoonPhase moonPhase = GetLunarDay(time).GetMoonPhase();
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, PlanetName.Sun);
        Angle planetNirayanaLongitude2 = GetPlanetNirayanaLongitude(time, PlanetName.Moon);
        Angle distanceBetweenPlanets = GetDistanceBetweenPlanets(planetNirayanaLongitude2, planetNirayanaLongitude);
        if (distanceBetweenPlanets.TotalDegrees > 180.0)
        {
            distanceBetweenPlanets = GetDistanceBetweenPlanets(distanceBetweenPlanets, Angle.Degrees360);
        }

        double num2 = 0.0;
        switch (moonPhase)
        {
            case MoonPhase.BrightHalf:
                num2 = distanceBetweenPlanets.TotalDegrees / 3.0;
                break;
            case MoonPhase.DarkHalf:
                {
                    double totalDegrees = GetDistanceBetweenPlanets(distanceBetweenPlanets, Angle.Degrees360).TotalDegrees;
                    num2 = totalDegrees / 3.0;
                    break;
                }
        }

        double num3 = 60.0 - num2;
        bool flag = IsPlanetMalefic(planetName, time);
        bool flag2 = IsPlanetBenefic(planetName, time);
        if (flag2 && !flag)
        {
            num = num2;
        }

        if (!flag2 && flag)
        {
            num = num3;
        }

        if (planetName == PlanetName.Moon)
        {
            num *= 2.0;
        }

        if (num == 0.0)
        {
            throw new Exception("Paksha bala not found, error!");
        }

        return new Shashtiamsa(num);
    }

    public static Shashtiamsa GetPlanetNathonnathaBala(PlanetName planetName, Time time)
    {
        DateTime localApparentTime = GetLocalApparentTime(time);
        int hour = localApparentTime.Hour;
        double num = (double)localApparentTime.Minute / 60.0;
        double num2 = (double)localApparentTime.Second / 3600.0;
        double num3 = (double)hour + num + num2;
        double num4 = num3 * 15.0;
        if (num4 > 180.0)
        {
            num4 = 360.0 - num4;
        }

        if (planetName == PlanetName.Sun || planetName == PlanetName.Jupiter || planetName == PlanetName.Venus)
        {
            double shashtiamsa = num4 / 3.0;
            return new Shashtiamsa(shashtiamsa);
        }

        if (planetName == PlanetName.Saturn || planetName == PlanetName.Moon || planetName == PlanetName.Mars)
        {
            double shashtiamsa2 = (180.0 - num4) / 3.0;
            return new Shashtiamsa(shashtiamsa2);
        }

        if (planetName == PlanetName.Mercury)
        {
            return new Shashtiamsa(60.0);
        }

        throw new Exception("Planet Nathonnatha Bala not found, error!");
    }

    public static Shashtiamsa GetPlanetDigBala(PlanetName planetName, Time time)
    {
        Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time, planetName);
        Angle planet = null;
        if (planetName == PlanetName.Sun || planetName == PlanetName.Mars)
        {
            planet = GetHouse(HouseName.House4, time).GetMiddleLongitude();
        }

        if (planetName == PlanetName.Jupiter || planetName == PlanetName.Mercury)
        {
            planet = GetHouse(HouseName.House7, time).GetMiddleLongitude();
        }

        if (planetName == PlanetName.Venus || planetName == PlanetName.Moon)
        {
            planet = GetHouse(HouseName.House10, time).GetMiddleLongitude();
        }

        if (planetName == PlanetName.Saturn)
        {
            planet = GetHouse(HouseName.House1, time).GetMiddleLongitude();
        }

        Angle distanceBetweenPlanets = GetDistanceBetweenPlanets(planetNirayanaLongitude, planet);
        if (distanceBetweenPlanets > Angle.Degrees180)
        {
            distanceBetweenPlanets = GetDistanceBetweenPlanets(distanceBetweenPlanets, Angle.Degrees360);
        }

        double shashtiamsa = distanceBetweenPlanets.TotalDegrees / 3.0;
        return new Shashtiamsa(shashtiamsa);
    }

    public static Shashtiamsa GetBhavabala(HouseName house, Time time)
    {
        Dictionary<string, Dictionary<HouseName, double>> dictionary = new Dictionary<string, Dictionary<HouseName, double>>
        {
            ["BhavaAdhipathiBala"] = CalcBhavaAdhipathiBala(time),
            ["BhavaDigBala"] = CalcBhavaDigBala(time),
            ["BhavaDrishtiBala"] = CalcBhavaDrishtiBala(time)
        };
        List<string> list = new List<string> { "BhavaAdhipathiBala", "BhavaDigBala", "BhavaDrishtiBala" };
        Dictionary<HouseName, double> dictionary2 = new Dictionary<HouseName, double>();
        foreach (HouseName allHouse in House.AllHouses)
        {
            double num = 0.0;
            foreach (string item in list)
            {
                num += dictionary[item][allHouse];
            }

            dictionary2[allHouse] = num;
        }

        return new Shashtiamsa(dictionary2[house]);
    }

    public static Dictionary<HouseName, double> CalcBhavaDrishtiBala(Time time)
    {
        Dictionary<PlanetName, int> dictionary2 = goodAndBad();
        double vdrishti;
        Dictionary<string, double> dictionary3 = GetDrishtiKendra(time);
        double num = 0.0;
        Dictionary<HouseName, double> dictionary4 = new Dictionary<HouseName, double>();
        foreach (HouseName allHouse in House.AllHouses)
        {
            num = 0.0;
            foreach (PlanetName all7Planet in PlanetName.All7Planets)
            {
                num += (double)dictionary2[all7Planet] * dictionary3[all7Planet.ToString() + allHouse];
            }

            dictionary4[allHouse] = num;
        }

        return dictionary4;
        Dictionary<string, double> GetDrishtiKendra(Time time1)
        {
            Dictionary<string, double> dictionary5 = new Dictionary<string, double>();
            foreach (PlanetName all7Planet2 in PlanetName.All7Planets)
            {
                foreach (HouseName allHouse2 in House.AllHouses)
                {
                    Angle middleLongitude = GetHouse(allHouse2, time1).GetMiddleLongitude();
                    Angle planetNirayanaLongitude = GetPlanetNirayanaLongitude(time1, all7Planet2);
                    double num2 = (middleLongitude - planetNirayanaLongitude).TotalDegrees;
                    if (num2 < 0.0)
                    {
                        num2 += 360.0;
                    }

                    vdrishti = FindViseshaDrishti(num2, all7Planet2);
                    if (all7Planet2 == PlanetName.Mercury || all7Planet2 == PlanetName.Jupiter)
                    {
                        dictionary5[all7Planet2.ToString() + allHouse2] = FindDrishtiValue(num2) + vdrishti;
                    }
                    else
                    {
                        dictionary5[all7Planet2.ToString() + allHouse2] = (FindDrishtiValue(num2) + vdrishti) / 4.0;
                    }
                }
            }

            return dictionary5;
        }

        Dictionary<PlanetName, int> goodAndBad()
        {
            Dictionary<PlanetName, int> dictionary = new Dictionary<PlanetName, int>();
            foreach (PlanetName all7Planet3 in PlanetName.All7Planets)
            {
                if (all7Planet3 == PlanetName.Mercury)
                {
                    dictionary[all7Planet3] = 1;
                }
                else if (IsPlanetBenefic(all7Planet3, time))
                {
                    dictionary[all7Planet3] = 1;
                }
                else
                {
                    dictionary[all7Planet3] = -1;
                }
            }

            return dictionary;
        }
    }

    public static Dictionary<HouseName, double> CalcBhavaDigBala(Time time)
    {
        Dictionary<HouseName, double> dictionary = new Dictionary<HouseName, double>();
        int num = 0;
        foreach (HouseName allHouse in House.AllHouses)
        {
            double totalDegrees = GetHouse(allHouse, time).GetMiddleLongitude().TotalDegrees;
            ZodiacName houseSignName = GetHouseSignName((int)allHouse, time);
            if (totalDegrees >= 210.0 && totalDegrees <= 240.0)
            {
                num = (int)(1 - allHouse);
            }
            else if ((totalDegrees >= 0.0 && totalDegrees <= 60.0) || (totalDegrees >= 120.0 && totalDegrees <= 150.0) || (totalDegrees >= 255.0 && totalDegrees <= 285.0))
            {
                num = (int)(4 - allHouse);
            }
            else if ((totalDegrees >= 60.0 && totalDegrees <= 90.0) || (totalDegrees >= 150.0 && totalDegrees <= 210.0) || (totalDegrees >= 300.0 && totalDegrees <= 330.0) || (totalDegrees >= 240.0 && totalDegrees <= 255.0))
            {
                num = (int)(7 - allHouse);
            }
            else if ((totalDegrees >= 90.0 && totalDegrees <= 120.0) || (totalDegrees >= 330.0 && totalDegrees <= 360.0) || (totalDegrees >= 285.0 && totalDegrees <= 300.0))
            {
                num = (int)(10 - allHouse);
            }

            if (num < 0)
            {
                num += 12;
            }

            if (num > 6)
            {
                num = 12 - num;
            }

            dictionary[allHouse] = (double)num * 10.0;
        }

        return dictionary;
    }

    public static Dictionary<HouseName, double> CalcBhavaAdhipathiBala(Time time)
    {
        Dictionary<HouseName, double> dictionary = new Dictionary<HouseName, double>();
        foreach (HouseName allHouse in House.AllHouses)
        {
            PlanetName lordOfHouse = GetLordOfHouse(allHouse, time);
            dictionary[allHouse] = GetPlanetShadbalaPinda(lordOfHouse, time).ToDouble();
        }

        return dictionary;
    }
}