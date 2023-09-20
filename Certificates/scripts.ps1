#https://engineering.circle.com/https-authorized-certs-with-node-js-315e548354a2
#https://www.golinuxcloud.com/openssl-subject-alternative-name/

$certDir = "$PSScriptRoot\certs"
$caConfigFile = "$PSScriptRoot\ca.cnf"
$caKeyFile = "$certDir\ca.key"
$caCrtFile = "$certDir\ca.crt"
$caPemFile = "$certDir\ca.pem"
$clientConfigFile = "$PSScriptRoot\client.cnf"
$clientKeyFile = "$certDir\client.key"
$clientCsrFile = "$certDir\client.csr"
$clientCrtFile = "$certDir\client.crt"
$clientPemFile = "$certDir\client.pem"
$signingConfigFile = "$PSScriptRoot\signing.cnf"
$signingtKeyFile = "$certDir\signing.key"
$signingCsrFile = "$certDir\signing.csr"
$signingCrtFile = "$certDir\signing.crt"
$signingPemFile = "$certDir\signing.pem"
$encriptingConfigFile = "$PSScriptRoot\signing.cnf"
$encriptingKeyFile = "$certDir\encripting.key"
$encriptingCsrFile = "$certDir\encripting.csr"
$encriptingCrtFile = "$certDir\encripting.crt"
$encriptingPemFile = "$certDir\encripting.pem"

$ErrorActionPreference = "SilentlyContinue"

function Test-UserIsAdmin{
    $currentPrincipal = New-Object System.Security.Principal.WindowsPrincipal([System.Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentPrincipal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-Openssl{
    try{
        openssl version
        return $true
    } 
    catch{
        retrun $false
    }
}
function Remove-TrustedCertificate{
    param(
        [string[]]$Thumbprints
    )
    
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
    
    try {
        $store.Open("ReadWrite")  
        foreach ($cert in $store.Certificates) {

            foreach($t in $Thumbprints){
                if($cert.Thumbprint -eq $t){
                    $store.Remove($cert)
                    Write-Host $cert.Thumbprint " removed"
                }  
            }
        }
    }
    finally  {
        $store.Close()
        $store.Dispose()
    }
}
function Get-TrustedCertificateBySubject{
    param(
        [string]$Subject
    )

    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")

    try {
        $store.Open("ReadOnly")  
        return  $store.Certificates | Where-Object Subject -eq $Subject | select -ExpandProperty Thumbprint
    }
    finally  { 
        $store.Close();
        $store.Dispose();
    }
}
function Remove-CertDir{
    if(Test-Path $certDir){
        Remove-Item -LiteralPath $certDir -Force -Recurse
    }
}
function Remove-CreatedCACertificatesFromTrustedRoot{
    Write-Host "Remove-CreatedCertificatesFromTrustedRoot"

    $createdCACert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $caCrtFile
    $certs = Get-TrustedCertificateBySubject($createdCaCert.Subject)
    Remove-TrustedCertificate($certs)
}
function Add-CertFolder{
    Write-Host "Creating directory"

    [System.IO.Directory]::CreateDirectory($certDir) | Out-Null
}
function Add-CaCert{
    openssl req -new -x509 -days 999 -config $caConfigFile -keyout $caKeyFile -out $caCrtFile
    
    openssl x509 -in $caCrtFile -out $caPemFile -outform PEM
}


function Add-SigningCert{
    Write-Host "Add SigningCert"

    openssl genrsa -out $signingtKeyFile 4096

    openssl req -new -config $signingConfigFile -key $signingtKeyFile -out $signingCsrFile

    openssl x509 -req -extfile $signingConfigFile -days 999 -passin "pass:password" -in $signingCsrFile `
    -CA $caCrtFile -CAkey $caKeyFile -CAcreateserial -out $signingCrtFile -extensions req_ext -extfile $signingConfigFile

    openssl x509 -in $signingCrtFile -out $signingPemFile -outform PEM
}

function Add-EncryptingCert{
    Write-Host "Add EncriptingCert"

    openssl genrsa -out $encriptingKeyFile 4096

    openssl req -new -config $signingConfigFile -key $encriptingKeyFile -out $encriptingCsrFile

    openssl x509 -req -extfile $encriptingConfigFile -days 999 -passin "pass:password" -in $encriptingCsrFile `
    -CA $caCrtFile -CAkey $caKeyFile -CAcreateserial -out $encriptingCrtFile -extensions req_ext -extfile $encriptingConfigFile

    openssl x509 -in $encriptingCrtFile -out $encriptingPemFile -outform PEM
}


function Test-SANExtentionsInTheSigningCert{ #if valid result should not be  empty

    Write-Host "Test SANExtentionsInTheSigningCert"

    openssl x509 -text -noout -in $signingCrtFile | grep -A 1 "Subject Alternative Name"
}

function Test-SANExtentionsInTheEncriptingCert{ #if valid result should not be  empty

    Write-Host "Test SANExtentionsInTheEncriptingCert"

    openssl x509 -text -noout -in $encriptingCrtFile | grep -A 1 "Subject Alternative Name"
}

function Add-ClientCert{
    Write-Host "Add ClientCert"

    openssl genrsa -out $clientKeyFile 4096

    openssl req -new -config $clientConfigFile -key $clientKeyFile -out $clientCsrFile

    openssl x509 -req -extfile $clientConfigFile -days 999 -passin "pass:password" -in $clientCsrFile `
    -CA $caCrtFile -CAkey $caKeyFile -CAcreateserial -out $clientCrtFile -extensions req_ext -extfile $clientConfigFile

    openssl x509 -in $clientCrtFile -out $clientPemFile -outform PEM
}


function Test-SANExtentionsInTheClientCert{ #if valid result should not be  empty

    Write-Host "Test SANExtentionsInTheClientCert"

    openssl x509 -text -noout -in $clientCrtFile | grep -A 1 "Subject Alternative Name"
}
function Add-CAToTrustedRootAuthorities{
    Write-Host "Add ToTrustedRootAuthorities"

    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $caCrtFile

    try {
        $store.Open("ReadWrite")

        $certAlreadyTrusted = ($store.Certificates | Where-Object  Subject -eq $cert.Subject | Measure-Object).Count -ge 1

        if($certAlreadyTrusted -eq $false){
            Write-Host "Add to trusted root certification authorities"
            $store.Add($cert)
        }   
        else{
            Write-Host "Certificate already exist in the trusted root certification authorities"
        }
    }
    catch{
        Write-Host "Add ToTrustedRootAuthorities Error occured"
    }
    finally {
        $store.Close()
        $store.Dispose()
    }
}


if((Test-UserIsAdmin) -eq $false){
    Write-Host "You should run script as Admin."
    Exit  
}

if((Test-Openssl) -eq $false ){
    Write-Host "Cannot find openssl !!!"
    Exit 
}

Add-CertFolder
Add-CaCert

Add-ClientCert
Test-SANExtentionsInTheClientCert

Add-SigningCert
Add-EncryptingCert
Test-SANExtentionsInTheSigningCert
Test-SANExtentionsInTheEncriptingCert

Remove-CreatedCACertificatesFromTrustedRoot
Add-CAToTrustedRootAuthorities