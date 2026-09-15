import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:dio/dio.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';

import '../models/item_models.dart';
import '../services/items_service.dart';

class AddPhotoScreen extends StatefulWidget {
  final String itemId;

  const AddPhotoScreen({super.key, required this.itemId});

  @override
  State<AddPhotoScreen> createState() => _AddPhotoScreenState();
}

class _AddPhotoScreenState extends State<AddPhotoScreen> {
  final ItemsService _itemsService = ItemsService();
  final ImagePicker _picker = ImagePicker();
  
  File? _selectedImage;
  bool _isSubmitting = false;

  Future<void> _pickImage(ImageSource source) async {
    try {
      final XFile? pickedFile = await _picker.pickImage(
        source: source,
        imageQuality: 80,
      );

      if (pickedFile != null) {
        setState(() {
          _selectedImage = File(pickedFile.path);
        });
      }
    } on PlatformException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Permission denied or error accessing ${source == ImageSource.camera ? "camera" : "gallery"}: ${e.message}'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Failed to select image.'),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  void _cancelSelection() {
    setState(() {
      _selectedImage = null;
    });
  }

  Future<void> _submit() async {
    if (_selectedImage == null) return;

    setState(() {
      _isSubmitting = true;
    });

    try {
      // Mock upload delay to simulate real network conditions
      await Future.delayed(const Duration(seconds: 1));

      // Use a placeholder URL to satisfy the API contract (since there's no actual upload endpoint)
      const mockedUploadedUrl = 'https://dummyimage.com/600x400/000/fff&text=Uploaded+Image';

      final request = AddPhotoRequest(
        imageUrl: mockedUploadedUrl,
      );

      await _itemsService.addPhoto(widget.itemId, request);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Photo added successfully')),
        );
        context.pop(true);
      }
    } on DioException catch (e) {
      if (mounted) {
        String errorMsg = 'Failed to add photo.';
        if (e.response?.statusCode == 400) {
          errorMsg = 'Validation error: ${e.response?.data}';
        }
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(errorMsg), backgroundColor: Colors.red),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('An unexpected error occurred.'), backgroundColor: Colors.red),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  Widget _buildEmptyState() {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const Icon(Icons.add_a_photo, size: 64, color: Colors.grey),
        const SizedBox(height: 16),
        const Text(
          'No photo selected',
          textAlign: TextAlign.center,
          style: TextStyle(fontSize: 18, color: Colors.grey),
        ),
        const SizedBox(height: 32),
        ElevatedButton.icon(
          onPressed: () => _pickImage(ImageSource.camera),
          icon: const Icon(Icons.camera_alt),
          label: const Text('Take a Photo'),
          style: ElevatedButton.styleFrom(
            padding: const EdgeInsets.symmetric(vertical: 16),
          ),
        ),
        const SizedBox(height: 16),
        OutlinedButton.icon(
          onPressed: () => _pickImage(ImageSource.gallery),
          icon: const Icon(Icons.photo_library),
          label: const Text('Choose from Gallery'),
          style: OutlinedButton.styleFrom(
            padding: const EdgeInsets.symmetric(vertical: 16),
          ),
        ),
      ],
    );
  }

  Widget _buildPreviewState() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Expanded(
          child: ClipRRect(
            borderRadius: BorderRadius.circular(8.0),
            child: Image.file(
              _selectedImage!,
              fit: BoxFit.cover,
            ),
          ),
        ),
        const SizedBox(height: 24),
        if (_isSubmitting)
          const Center(child: CircularProgressIndicator())
        else ...[
          ElevatedButton.icon(
            onPressed: _submit,
            icon: const Icon(Icons.cloud_upload),
            label: const Text('Upload Photo'),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
            ),
          ),
          const SizedBox(height: 12),
          OutlinedButton(
            onPressed: _cancelSelection,
            style: OutlinedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
            ),
            child: const Text('Choose Another or Cancel'),
          ),
        ],
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Add Photo'),
      ),
      body: Padding(
        padding: const EdgeInsets.all(24.0),
        child: _selectedImage == null
            ? _buildEmptyState()
            : _buildPreviewState(),
      ),
    );
  }
}
