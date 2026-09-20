import 'package:flutter/material.dart';
import 'add_item_screen.dart';
import 'item_details_screen.dart';

class MyItemsScreen extends StatelessWidget {
  const MyItemsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('My Items')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            child: ListTile(
              title: const Text('Old iPhone'),
              subtitle: const Text('Draft'),
              onTap: () {
                Navigator.push(context, MaterialPageRoute(builder: (_) => const ItemDetailsScreen()));
              },
            ),
          )
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          Navigator.push(context, MaterialPageRoute(builder: (_) => const AddItemScreen()));
        },
        child: const Icon(Icons.add),
      ),
    );
  }
}
