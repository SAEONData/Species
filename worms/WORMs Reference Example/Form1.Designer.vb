<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnQueryTaxa = New System.Windows.Forms.Button()
        Me.lstRecords = New System.Windows.Forms.ListBox()
        Me.SuspendLayout()
        '
        'btnQueryTaxa
        '
        Me.btnQueryTaxa.Location = New System.Drawing.Point(12, 12)
        Me.btnQueryTaxa.Name = "btnQueryTaxa"
        Me.btnQueryTaxa.Size = New System.Drawing.Size(75, 23)
        Me.btnQueryTaxa.TabIndex = 0
        Me.btnQueryTaxa.Text = "Query Taxa"
        Me.btnQueryTaxa.UseVisualStyleBackColor = True
        '
        'lstRecords
        '
        Me.lstRecords.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstRecords.FormattingEnabled = True
        Me.lstRecords.Location = New System.Drawing.Point(12, 42)
        Me.lstRecords.Name = "lstRecords"
        Me.lstRecords.Size = New System.Drawing.Size(260, 199)
        Me.lstRecords.TabIndex = 1
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 262)
        Me.Controls.Add(Me.lstRecords)
        Me.Controls.Add(Me.btnQueryTaxa)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnQueryTaxa As System.Windows.Forms.Button
    Friend WithEvents lstRecords As System.Windows.Forms.ListBox

End Class
