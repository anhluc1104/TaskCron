Module Module1

    Sub Main()
        Console.WriteLine("Start")
        TaskSchedule.cron_get_retela_supplier_master.CallAPI()
        Console.WriteLine("OK")
    End Sub

End Module
