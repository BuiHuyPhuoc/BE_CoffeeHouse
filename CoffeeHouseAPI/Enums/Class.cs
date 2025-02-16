﻿namespace CoffeeHouseAPI.Enums
{
    public enum API_ACTION
    {
        GET = 0,
        POST = 1,
        PUT = 2,
        DELETE = 3,
    }

    public enum ORDER_STATUS
    {
        BOOKED,
        CANCELED,
        DELIVERED,
        DELIVERING,
        INPROCESSING
    }
}
