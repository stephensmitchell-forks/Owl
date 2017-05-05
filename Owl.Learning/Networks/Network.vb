﻿Imports Owl.Learning.Initializers
Imports Owl.Learning.NeuronFunctions

Namespace Networks

    Public Class Network
        Inherits NetworkBase

        Private _weights As New TensorSet
        Private _biases As New TensorSet
        Private _neur As New List(Of NeuronFunctionBase)

        ''' <summary>
        ''' The hardcore way.
        ''' </summary>
        Public Sub New()

        End Sub

        ''' <summary>
        ''' The direct way.
        ''' </summary>
        Public Sub New(Weights As TensorSet, Biases As TensorSet, NeuronFunctions As IEnumerable(Of NeuronFunctionBase))
            Me.Weights = Weights
            Me.Biases = Biases
            Me.NeuronFunctions.AddRange(NeuronFunctions)
        End Sub

        ''' <summary>
        ''' The usual way.
        ''' </summary>
        ''' <param name="NFunction"></param>
        ''' <param name="Inputs"></param>
        ''' <param name="Neurons"></param>
        ''' <param name="NetworkInitializer"></param>
        Sub New(NFunction As NeuronFunctionBase, Inputs As Integer, Neurons As IEnumerable(Of Integer), Optional NetworkInitializer As InitializerBase = Nothing)
            For i As Integer = 0 To Neurons.Count - 1 Step 1
                _neur.Add(NFunction.Duplicate)
            Next

            Dim nl As New List(Of Integer) From {Inputs}
            nl.AddRange(Neurons)

            For i As Integer = 1 To nl.Count - 1 Step 1
                Dim prev As Integer = nl(i - 1)
                Dim this As Integer = nl(i)

                _weights.Add(New Tensor(New List(Of Integer) From {prev, this}))
                _biases.Add(New Tensor(New List(Of Integer) From {this}))
            Next

            If NetworkInitializer IsNot Nothing Then NetworkInitializer.InitializeNetwork(Me)
        End Sub

        ''' <summary>
        ''' The usual way.
        ''' </summary>
        ''' <param name="NFunctions"></param>
        ''' <param name="Inputs"></param>
        ''' <param name="Neurons"></param>
        ''' <param name="NetworkInitializer"></param>
        Sub New(NFunctions As IEnumerable(Of NeuronFunctionBase), Inputs As Integer, Neurons As IEnumerable(Of Integer), Optional NetworkInitializer As InitializerBase = Nothing)
            _neur.AddRange(NFunctions)

            Dim nl As New List(Of Integer) From {Inputs}
            nl.AddRange(Neurons)

            For i As Integer = 1 To nl.Count - 1 Step 1
                Dim prev As Integer = nl(i - 1)
                Dim this As Integer = nl(i)

                _weights.Add(New Tensor(New List(Of Integer) From {prev, this}))
                _biases.Add(New Tensor(New List(Of Integer) From {this}))
            Next

            If NetworkInitializer IsNot Nothing Then NetworkInitializer.InitializeNetwork(Me)
        End Sub

        Public Function Duplicate() As Network
            Dim nl As New List(Of NeuronFunctionBase)
            For Each f As NeuronFunctionBase In Me.NeuronFunctions
                nl.Add(f.Duplicate)
            Next
            Return New Network(Weights.Duplicate, Biases.Duplicate, nl)
        End Function

        Public Function NeuronCounts() As List(Of Integer)
            Dim nl As New List(Of Integer)
            For i = 0 To Me.LayerCount - 1
                nl.Add(Me.NeuronCount(i))
            Next
            Return nl
        End Function

        Public ReadOnly Property LayerCount As Integer
            Get
                Return _weights.Count
            End Get
        End Property

        Public ReadOnly Property InputCount(LayerIndex As Integer) As Integer
            Get
                Return _weights(LayerIndex).ShapeAt(0)
            End Get
        End Property

        Public ReadOnly Property NeuronCount(LayerIndex As Integer) As Integer
            Get
                Return _weights(LayerIndex).ShapeAt(1)
            End Get
        End Property

        Public Property LayerWeights(layerIndex As Integer) As Tensor
            Get
                Return _weights(layerIndex)
            End Get
            Set(value As Tensor)
                _weights(layerIndex) = value
            End Set
        End Property

        Public Property LayerBiases(layerIndex As Integer) As Tensor
            Get
                Return _biases(layerIndex)
            End Get
            Set(value As Tensor)
                _biases(layerIndex) = value
            End Set
        End Property

        ''' <summary>
        ''' Direct access to the underlying arrays. Weights are stored in Tensors of form [Input, Neuron].
        ''' This implies the input Tensor for Compute has to be of shape [1, Input].
        ''' The ComputeLayer performs this reshaping before feeding the Tensor.
        ''' </summary>
        ''' <returns></returns>
        Public Property Weights As TensorSet
            Get
                Return _weights
            End Get
            Set(value As TensorSet)
                _weights = value
            End Set
        End Property

        ''' <summary>
        ''' Direct access to the underlying arrays
        ''' </summary>
        ''' <returns></returns>
        Public Property Biases As TensorSet
            Get
                Return _biases
            End Get
            Set(value As TensorSet)
                _biases = value
            End Set
        End Property

        ''' <summary>
        ''' Direct access to the underlying function per layer.
        ''' </summary>
        ''' <returns></returns>
        Public Property NeuronFunctions As List(Of NeuronFunctionBase)
            Get
                Return _neur
            End Get
            Set(value As List(Of NeuronFunctionBase))
                _neur = value
            End Set
        End Property

        Public Overrides Function Compute(InputTensor As Tensor) As Tensor
            Dim thistens As Tensor = InputTensor
            For i As Integer = 0 To Me.LayerCount - 1 Step 1
                thistens = ComputeLayer(thistens, i)
            Next
            Return thistens
        End Function

        ''' <summary>
        ''' Input tensor gets always reshaped into a [1, length] matrix.
        ''' </summary>
        ''' <param name="InputTensor"></param>
        ''' <param name="LayerIndex"></param>
        ''' <returns></returns>
        Public Function ComputeLayer(InputTensor As Tensor, LayerIndex As Integer) As Tensor
            InputTensor.TryReshape({1, InputTensor.Height})
            Dim tsum As Tensor = Tensor.MatMul(InputTensor, Me.Weights(LayerIndex))
            tsum.Add(Biases(LayerIndex))
            NeuronFunctions(LayerIndex).Evaluate(tsum)
            Return tsum
        End Function

    End Class

End Namespace

