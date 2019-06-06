<?php

    ini_set('display_errors',1);
    ini_set('display_startup_errors',1);
    error_reporting(E_ALL);
    
    $client = new SoapClient("http://www.marinespecies.org/aphia.php?p=soap&wsdl=1");
    // var_dump($client->__getFunctions());

    $name = "black mussel";
    $records = $client->getAphiaRecordsByVernacular($name, false, 0);
    
    $size = count($records);
    for ($i=0; $i<$size; $i++)
    {
        $record = $records[$i];    
        $commonName = $client->getAphiaVernacularsByID($record->AphiaID);
        $record->vernacular = $commonName[0]->vernacular;
    }
    
    $text = json_encode($records);
    die($text);
    
    $size = count($records);
    for ($i=0; $i<$size; $i++)
    {
        $record = $records[$i];    
        echo("vernacular: " . $record->vernacular . "<br>");
        echo("AphiaID: " . $record->AphiaID . "<br>");
        echo("url: " . $record->url . "<br>");
        echo("scientificname: " . $record->scientificname . "<br>");
        echo("authority: " . $record->authority . "<br>");
        echo("rank: " . $record->rank . "<br>");
        echo("status: " . $record->status . "<br>");
        echo("valid_name: " . $record->valid_name . "<br>");
        echo("valid_authority: " . $record->valid_authority . "<br>");
        echo("kingdom: " . $record->kingdom . "<br>");
        echo("phylum: " . $record->phylum . "<br>");
        echo("class: " . $record->class . "<br>");
        echo("order: " . $record->order . "<br>");
        echo("family: " . $record->family . "<br>");
        echo("genus: " . $record->genus . "<br>");
        echo("citation: " . $record->citation . "<br>");
        echo("lsid: " . $record->lsid . "<br>");
        echo("isMarine: " . $record->isMarine . "<br>");
        echo("<br><br>");
    }

?>
