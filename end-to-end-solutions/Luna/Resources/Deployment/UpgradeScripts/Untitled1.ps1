$connectionString = "Server=tcp:lunauitest-sqlserver.database.windows.net,1433;Initial Catalog=lunauitest-sqldb;Persist Security Info=False;User ID=lunauserlunauitest;Password='|d-z$"+"("+"4T/>X!CQCNjNkop^jd';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$vars = $connectionString.Split(";")

foreach ($var in $vars){
    $pair = $var.Split("=")
    $pair[0]
    $pair[1]

    if ($pair[0] -eq 'Server'){
        $serverInstance = ''''+$pair[1].Substring(4).Replace(",1433", "")+''''
    }
    elseif ($pair[0] -eq 'Initial Catalog'){
        $database = ''''+$pair[1] + ''''
    }
    elseif ($pair[0] -eq 'User ID'){
        $userName = ''''+$pair[1]+''''
    }
    elseif ($pair[0] -eq 'Password'){
        $password = $pair[1]
    }
}


$serverInstance
$userName
$password
$database


