﻿using System;
using System.Collections.Generic;

namespace door.Domain.Entities;

public class DataEntry
{
    public int Id { get; private set; }
    public string Date { get; private set; }
    public string Time { get; private set; }
    public int DoorStatusId { get; private set; }
    public MasterDoorStatus DoorStatus { get; private set; } // ナビゲーションプロパティ

    private DataEntry() { }

    public DataEntry(string date, string time, int doorStatusId) //int id, // , MasterDoorStatus doorStatus
    {
        //Id = id;
        Date = date;
        Time = time;
        DoorStatusId = doorStatusId;
        //DoorStatus = doorStatus;
    }

    // ✅ テスト用のファクトリメソッド
    public static DataEntry CreateForTest(int id, string date, string time, int doorStatusId)
    {
        return new DataEntry(date, time, doorStatusId) { Id = id };
    }
}